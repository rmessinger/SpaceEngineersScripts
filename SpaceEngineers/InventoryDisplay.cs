using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace Utilities
{
    public class InventoryDisplay : MyGridProgram
    {
        IMyCubeGrid baseGrid;
        IMyTextPanel inventoryDisplay;
        ISet<IMyRefinery> refineries;
        ISet<IMyAssembler> assemblers;
        ISet<IMyCargoContainer> cargoContainers;
        IDictionary<string, MyItemType> ingotTypes;

        public InventoryDisplay()
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
                    break;
                }
            }

            inventoryDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
            inventoryDisplay.BackgroundColor = new Color(0f);

            cargoContainers = FindCargoContainers();
            refineries = FindRefineries();
            assemblers = FindAssemblers();

            ingotTypes.Add("Iron", MyItemType.MakeIngot("Iron"));
            ingotTypes.Add("Cobalt", MyItemType.MakeIngot("Cobalt"));
        }

        public void Main(string argument, UpdateType updateSource)
        {

            float ironIngotCount = 0;
            // TODO this shouldn't be 3 loops... they all have base types don't they
            foreach (IMyCargoContainer container in cargoContainers)
            {
                // TODO cache these
                IMyInventory containerInventory = container.GetInventory();
                MyInventoryItem? ironIngots = containerInventory.FindItem(MyItemType.MakeIngot("Iron"));

                if (ironIngots != null)
                {
                    Echo("Cargo: adding " + (float)ironIngots?.Amount.RawValue / 1000000000f);
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / 1000000000f;
                }
            }

            foreach (IMyRefinery refinery in refineries)
            {
                // TODO cache these
                IMyInventory refineryInventory = refinery.OutputInventory;
                MyInventoryItem? ironIngots = refineryInventory.FindItem(MyItemType.MakeIngot("Iron"));

                if (ironIngots != null)
                {
                    Echo("Refinery: adding " + (float)ironIngots?.Amount.RawValue / 1000000000f);
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / 1000000000f;
                }
            }

            foreach (IMyAssembler assembler in assemblers)
            {
                // TODO cache these
                IMyInventory assemblerInventory = assembler.InputInventory;
                MyInventoryItem? ironIngots = assemblerInventory.FindItem(MyItemType.MakeIngot("Iron"));

                if (ironIngots != null)
                {
                    Echo("Assembler: adding " + (float)ironIngots?.Amount.RawValue / 1000000000f);
                    ironIngotCount += (float)ironIngots?.Amount.RawValue / 1000000000f;
                }
            }

            inventoryDisplay.WriteText("Iron Ingots: " + Math.Round(ironIngotCount, 3) + "k");
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
