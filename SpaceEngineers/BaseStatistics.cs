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
        IMyTextPanel capacityDisplay;
        ISet<IMyRefinery> refineries;
        ISet<IMyAssembler> assemblers;
        ISet<IMyCargoContainer> cargoContainers;
        IDictionary<string, MyItemType> ingotTypes;
        System.Text.RegularExpressions.Regex maxInputRegex;

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
                if (panel.CustomName == "Power Display")
                {
                    capacityDisplay = panel;
                    capacityDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
                    capacityDisplay.BackgroundColor = new Color(0f);
                    break;
                }
            }

            cargoContainers = FindCargoContainers();
            refineries = FindRefineries();
            assemblers = FindAssemblers();

            ingotTypes = new Dictionary<string, MyItemType>();
            ingotTypes.Add("Iron", MyItemType.MakeIngot("Iron"));
            ingotTypes.Add("Cobalt", MyItemType.MakeIngot("Cobalt"));
        }

        public void Main(string argument, UpdateType updateSource)
        {

            float ironIngotCount = 0;
            float cobaltIngotCount = 0;
            string refineryData = string.Empty;
            // TODO this shouldn't be 3 loops... they all have base types don't they
            foreach (IMyCargoContainer container in cargoContainers)
            {
                // TODO cache these
                IMyInventory containerInventory = container.GetInventory();
                MyInventoryItem? ironIngots = containerInventory.FindItem(ingotTypes["Iron"]);
                MyInventoryItem? cobaltIngots = containerInventory.FindItem(ingotTypes["Cobalt"]);

                // refactor into method
                if (ironIngots != null)
                {
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / 1000000000f;
                }
                if (cobaltIngots != null)
                {
                    cobaltIngotCount += (float)cobaltIngots?.Amount.RawValue / 1000000000f;
                }
            }

            foreach (IMyRefinery refinery in refineries)
            {
                // TODO cache these
                IMyInventory refineryInventory = refinery.OutputInventory;
                MyInventoryItem? ironIngots = refineryInventory.FindItem(ingotTypes["Iron"]);
                MyInventoryItem? cobaltIngots = refineryInventory.FindItem(ingotTypes["Cobalt"]);

                if (refineryData == string.Empty)
                {
                    refineryData = refinery.DetailedInfo;
                }
                if (ironIngots != null)
                {
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / 1000000000f;
                }
                if (cobaltIngots != null)
                {
                    cobaltIngotCount += (float)cobaltIngots?.Amount.RawValue / 1000000000f;
                }
            }

            foreach (IMyAssembler assembler in assemblers)
            {
                // TODO cache these
                IMyInventory assemblerInventory = assembler.InputInventory;
                MyInventoryItem? ironIngots = assemblerInventory.FindItem(ingotTypes["Iron"]);
                MyInventoryItem? cobaltIngots = assemblerInventory.FindItem(ingotTypes["Cobalt"]);

                if (ironIngots != null)
                {
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / 1000000000f;
                }
                if (cobaltIngots != null)
                {
                    cobaltIngotCount += (float)cobaltIngots?.Amount.RawValue / 1000000000f;
                }
            }

            inventoryDisplay.WriteText("Iron Ingots: " + Math.Round(ironIngotCount, 3) + "k\n");
            inventoryDisplay.WriteText("Cobalt Ingots: " + Math.Round(cobaltIngotCount, 3) + "k", true);
            capacityDisplay.WriteText(refineryData);
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
