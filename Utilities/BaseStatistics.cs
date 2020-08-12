using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace Utilities
{
    public class BaseStatistics : MyGridProgram
    {
        IMyCubeGrid baseGrid;
        IMyTextPanel inventoryDisplay;
        ISet<IMyRefinery> refineries;
        ISet<IMyAssembler> assemblers;
        ISet<IMyCargoContainer> cargoContainers;
        IDictionary<string, MyItemType> itemTypes;
        System.Text.RegularExpressions.Regex maxInputRegex;
        int milliToMegaScale = 1000000000;
        int lastArgHash = 0;
        string panelName = string.Empty;

        // Arguments separated by commas
        // Display name
        // Components
        public BaseStatistics()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            baseGrid = Me.CubeGrid;
            List<IMyTextPanel> allPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(allPanels);
            foreach (IMyTextPanel panel in allPanels)
            {
                if (panel.CustomName == "Inventory Display")
                {
                    inventoryDisplay = panel;
                    inventoryDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
                    inventoryDisplay.BackgroundColor = new Color(0f);
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
            float ironIngotCount = 0;
            float cobaltIngotCount = 0;

            int argHash = argument.GetHashCode();

            // New arguments, parse 'em
            if (argHash != lastArgHash)
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

            // TODO this shouldn't be 3 loops... they all have base types don't they
            foreach (IMyCargoContainer container in cargoContainers)
            {
                // TODO cache these
                IMyInventory containerInventory = container.GetInventory();
                MyInventoryItem? ironIngots = containerInventory.FindItem(itemTypes["Iron"]);
                MyInventoryItem? cobaltIngots = containerInventory.FindItem(itemTypes["Cobalt"]);

                // refactor into method
                if (ironIngots != null)
                {
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / milliToMegaScale;
                }
                if (cobaltIngots != null)
                {
                    cobaltIngotCount += (float)cobaltIngots?.Amount.RawValue / milliToMegaScale;
                }
            }

            foreach (IMyRefinery refinery in refineries)
            {
                // TODO cache these
                IMyInventory refineryInventory = refinery.OutputInventory;
                MyInventoryItem? ironIngots = refineryInventory.FindItem(itemTypes["Iron"]);
                MyInventoryItem? cobaltIngots = refineryInventory.FindItem(itemTypes["Cobalt"]);

                if (ironIngots != null)
                {
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / milliToMegaScale;
                }
                if (cobaltIngots != null)
                {
                    cobaltIngotCount += (float)cobaltIngots?.Amount.RawValue / milliToMegaScale;
                }
            }

            foreach (IMyAssembler assembler in assemblers)
            {
                // TODO cache these
                IMyInventory assemblerInventory = assembler.InputInventory;
                MyInventoryItem? ironIngots = assemblerInventory.FindItem(itemTypes["Iron"]);
                MyInventoryItem? cobaltIngots = assemblerInventory.FindItem(itemTypes["Cobalt"]);

                if (ironIngots != null)
                {
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / milliToMegaScale;
                }
                if (cobaltIngots != null)
                {
                    cobaltIngotCount += (float)cobaltIngots?.Amount.RawValue / milliToMegaScale;
                }
            }

            inventoryDisplay.WriteText("Iron Ingots: " + Math.Round(ironIngotCount, 3) + "k\n");
            inventoryDisplay.WriteText("Cobalt Ingots: " + Math.Round(cobaltIngotCount, 3) + "k", true);
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
