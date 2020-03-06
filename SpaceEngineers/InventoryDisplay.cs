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
        }
    }
}
