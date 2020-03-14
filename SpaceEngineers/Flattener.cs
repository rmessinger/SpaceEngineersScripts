using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace Utilities
{
    public class Flattener : MyGridProgram
    {
        IMyMotorAdvancedRotor rotor = null;
        ISet<IMyPistonBase> pistons;
        ISet<IMyShipDrill> drills;

        int minAngle = 180;
        int maxAngle = 345;

        public Flattener()
        {
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (rotor == null)
            {
                rotor = findRotor();
            }


        }

        private IMyMotorAdvancedRotor findRotor()
        {
            IMyMotorAdvancedRotor ret;
            List<IMyMotorAdvancedRotor> allRotors = new List<IMyMotorAdvancedRotor>();
            GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedRotor>(allRotors);
            foreach(IMyMotorAdvancedRotor rotor in allRotors)
            {
                if (rotor.CubeGrid.IsSameConstructAs(Me.CubeGrid))
                {
                    continue;
                }
                else if (rotor.DisplayName.Contains("Flattener"))
                {
                    ret = rotor;
                    break;
                }
            }

            return ret;
        }
    }
}
