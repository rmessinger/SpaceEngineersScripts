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

        public InventoryDisplay()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            baseGrid = Me.CubeGrid;
        }

        public void Main()
        {
            ISet<IMyCargoContainer> containers = FindCargoContainers();
            int ironIngots = 0;

            foreach (IMyCargoContainer container in containers)
            {
                // find iron here
            }
        }

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
    }
}
