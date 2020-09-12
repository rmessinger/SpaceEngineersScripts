using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace Utilities
{
    public class BaseStatistics : MyGridProgram
    {
        IMyCubeGrid baseGrid;
        IDictionary<int, IMyTextPanel> inventoryDisplays;
        ISet<IMyRefinery> refineries;
        ISet<IMyAssembler> assemblers;
        ISet<IMyCargoContainer> cargoContainers;
        IDictionary<string, MyItemType> itemTypes;
        int milliToMegaScale = 1000000000;
        int lastArgHash = 0;
        string panelName = string.Empty;
        int maxLines = 17;

        // Arguments separated by commas
        // Display name
        // Components
        public BaseStatistics()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            baseGrid = Me.CubeGrid;
            List<IMyTextPanel> allPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(allPanels);
            inventoryDisplays = new Dictionary<int, IMyTextPanel>();
            foreach (IMyTextPanel panel in allPanels)
            {
                if (panel.CustomName.Contains("Inventory Display"))
                {
                    int panelIndex = int.Parse(panel.CustomName[panel.CustomName.Length - 1].ToString());
                    panel.ContentType = ContentType.TEXT_AND_IMAGE;
                    panel.BackgroundColor = new Color(0f);
                    inventoryDisplays.Add(panelIndex, panel);
                }
            }

            cargoContainers = FindCargoContainers();
            refineries = FindRefineries();
            assemblers = FindAssemblers();

            itemTypes = new Dictionary<string, MyItemType>();
            itemTypes.Add("Iron", MyItemType.MakeIngot("Iron"));
            itemTypes.Add("Cobalt", MyItemType.MakeIngot("Cobalt"));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            DateTime start = DateTime.Now;
            MyFixedPoint currentVolume = 0;
            MyFixedPoint maxVolume = 0;

            int argHash = argument.GetHashCode();
            // New arguments, parse 'em
            if (argument != string.Empty && argHash != lastArgHash)
            {
                int index = 0;
                foreach (string rawArg in argument.Split(','))
                {
                    string arg = rawArg.Trim();
                    if (index == 0)
                    {
                        panelName = arg;
                    }
                    else
                    {
                        StringBuilder itemName = new StringBuilder();
                        // hahah this is stupid
                        // but I will not enforce case-sensitivity
                        if (arg.ToLower().Contains("ingot"))
                        {
                            arg.Replace("ingot", string.Empty);
                            itemName.Append(arg[0].ToString().ToUpper());
                            itemName.Append(arg.Substring(1).ToLower());
                        }
                    }

                    index++;
                }
            }

            
            IDictionary<string, MyFixedPoint> allItems = new Dictionary<string, MyFixedPoint>();
            List<IMyInventory> allInventories = new List<IMyInventory>();

            // TODO this shouldn't be 3 loops... they all have base types don't they
            foreach (IMyCargoContainer container in cargoContainers)
            {
                IMyInventory inventory = container.GetInventory();
                currentVolume += inventory.CurrentVolume;
                maxVolume += inventory.MaxVolume;
                allInventories.Add(inventory);
            }

            foreach (IMyRefinery refinery in refineries)
            {
                allInventories.Add(refinery.InputInventory);
                allInventories.Add(refinery.OutputInventory);
            }

            foreach (IMyAssembler assembler in assemblers)
            {
                allInventories.Add(assembler.InputInventory);
                allInventories.Add(assembler.OutputInventory);
            }

            foreach (IMyInventory inventory in allInventories)
            {
                List<MyInventoryItem> containerItems = new List<MyInventoryItem>();
                inventory.GetItems(containerItems);
                foreach (var item in containerItems)
                {
                    MyFixedPoint startingAmount = 0;
                    allItems.TryGetValue(item.Type.ToString(), out startingAmount);

                    allItems[item.Type.ToString()] = startingAmount + item.Amount;
                }
            }

            int linesWritten = 1;
            int currentPanelIndex = 1;
            IMyTextPanel currentPanel = inventoryDisplays[currentPanelIndex];
            SortedDictionary<string, MyFixedPoint> sortedItems = new SortedDictionary<string, MyFixedPoint>(allItems);
            // ironIngotCount += (float)ironIngots?.Amount.RawValue / milliToMegaScale;
            float utilization = ((float)currentVolume.RawValue / (float)maxVolume.RawValue) * 100;
            // currentPanel.WriteText($"Utilization: {currentVolume.RawValue} / {maxVolume.RawValue}\n", false);
            currentPanel.WriteText($"Utilization: {Math.Round(utilization, 3)}%\n", false);

            foreach (var item in sortedItems)
            {
                string[] itemFullName = item.Key.Split('/');

                currentPanel.WriteText(itemFullName[itemFullName.Length - 1] + ": " + item.Value + "\n", linesWritten > 0);
                linesWritten++;
                
                if (linesWritten >= maxLines)
                {
                    linesWritten = 0;
                    currentPanelIndex++;
                    currentPanel = inventoryDisplays[currentPanelIndex];
                }
            }

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Echo(duration.TotalMilliseconds+ "ms");
        }

        // TODO condense these into one
        private ISet<IMyCargoContainer> FindCargoContainers()
        {
            List<IMyCargoContainer> allCargoContainers = new List<IMyCargoContainer>();
            ISet<IMyCargoContainer> baseContainers = new HashSet<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(allCargoContainers);

            foreach(IMyCargoContainer container in allCargoContainers)
            {
                if (container.CubeGrid.IsSameConstructAs(baseGrid))
                {
                    baseContainers.Add(container);
                }
            }

            return baseContainers;
        }

        private ISet<IMyRefinery> FindRefineries()
        {
            List<IMyRefinery> allRefineries = new List<IMyRefinery>();
            ISet<IMyRefinery> baseRefineries = new HashSet<IMyRefinery>();
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(allRefineries);

            foreach (IMyRefinery container in allRefineries)
            {
                if (container.CubeGrid.IsSameConstructAs(baseGrid))
                {
                    baseRefineries.Add(container);
                }
            }

            return baseRefineries;
        }

        private ISet<IMyAssembler> FindAssemblers()
        {
            List<IMyAssembler> allAssemblers = new List<IMyAssembler>();
            ISet<IMyAssembler> baseAssemblers = new HashSet<IMyAssembler>();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(allAssemblers);

            foreach (IMyAssembler container in allAssemblers)
            {
                if (container.CubeGrid.IsSameConstructAs(baseGrid))
                {
                    baseAssemblers.Add(container);
                }
            }

            return baseAssemblers;
        }
    }
}
