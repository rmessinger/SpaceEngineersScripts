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
        enum FlatteningState
        {
            Unknown,
            Extending,
            Rotating
        }

        IMyMotorAdvancedStator rotor = null;
        ISet<IMyPistonBase> pistons;
        ISet<IMyShipDrill> drills;
        IMyPistonBase activePiston = null;
        FlatteningState state;

        int minAngle = 180;
        int maxAngle = 359;
        float startingExtension = 0;

        // extend pistons by 1 meter each time rotor reaches limit
        float extensionPerCycle = 1.0f;

        public Flattener()
        {
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (rotor == null && !findRotor())
            {
                Echo("No rotor found");
                return;
            }

            if ((pistons == null && pistons.Count == 0) && !findPistons())
            {
                Echo("No pistons found");
                return;
            }

            // Is the motor in action?
            if (rotor.RotorLock || !rotor.IsWorking || rotor.TargetVelocityRPM == 0)
            {
                Echo("ROTOR SEEMS DISABLED I GUESS?!");
                return;
            }

            // If state is unknown, well, HOW DID I GET HERE
            // AND THE DAYS GO BY
            if (state == FlatteningState.Unknown)
            {
                // determine what angle the rotor is turning
                // positive vel rpm means it's returning to home
                // hey, don't be ignorant. Negative vel can mean home too
                if (rotor.Angle <= minAngle || rotor.Angle >= maxAngle)
                {
                }
                else if (getPistonVelocity() != 0)
                {
                    // if the state is unknown and the pistons are moving, then sweet jesus stop it right now
                    stopAllPistons();
                    initiateRotatingState();
                }
                else
                {
                    // mid-point angle means rotating state
                    // set it and forget it my dude
                    state = FlatteningState.Rotating;
                }

            }
            else if (state == FlatteningState.Rotating)
            {
                // if the rotor has reached the target angle, start extending the pistons
                if (rotor.TargetVelocityRPM > 0 && rotor.Angle >= maxAngle)
                {
                    initiateExtendingState();
                }
            }
            else if (state == FlatteningState.Extending)
            {
                // check current position vs target
                if (getPistonExtension() >= startingExtension + extensionPerCycle)
                {
                    initiateRotatingState();
                }
                // check if the next piston in line needs to be activated
                else if (activePiston.CurrentPosition >= activePiston.MaxLimit)
                {
                    activePiston = getFirstUnmaxedPiston();
                    activePiston.Velocity = 0.1f;
                    activePiston.Enabled = true;
                }
            }
        }

        private bool findRotor()
        {
            IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName("Flattener Rotor");
            IMyMotorAdvancedStator rotorStatus = block as IMyMotorAdvancedStator;
            IMyMotorAdvancedRotor rotorControl = block as IMyMotorAdvancedRotor;

            if (rotorStatus == null || rotorControl == null)
            {
                return false;
            }
            else
            {
                this.rotor = rotorStatus;
                return true;
            }
        }

        private bool findPistons()
        {
            List<IMyPistonBase> allPistons = null;
            bool ret = false;
            this.GridTerminalSystem.GetBlocksOfType(allPistons);

            foreach(IMyPistonBase piston in allPistons)
            {
                if (!Me.CubeGrid.IsSameConstructAs(piston.CubeGrid))
                {
                    continue;
                }
                else if (piston.DisplayName.Contains("Flattener"))
                {
                    pistons.Add(piston);
                    ret = true;
                }
            }

            return ret;
        }

        private bool findDrills()
        {
            List<IMyShipDrill> allDrills = null;
            bool ret = false;
            this.GridTerminalSystem.GetBlocksOfType(allDrills);

            foreach(IMyShipDrill drill in allDrills)
            {
                if (!Me.CubeGrid.IsSameConstructAs(drill.CubeGrid))
                {
                    continue;
                }
                else if (drill.DisplayName.Contains("Flattener"))
                {
                    drills.Add(drill);
                    ret = true;
                }
            }

            return true;
        }

        private float getPistonExtension()
        {
            float ret = 0;

            foreach (IMyPistonBase piston in pistons)
            {
                ret += piston.CurrentPosition;
            }

            return ret;
        }

        private void activateDrills()
        {
            foreach (IMyShipDrill drill in drills)
            {
                drill.Enabled = true;
            }
        }

        private float getPistonVelocity()
        {
            float ret = 0;

            foreach (IMyPistonBase piston in pistons)
            {
                ret += piston.Velocity;
            }

            return ret;
        }

        private void stopAllPistons()
        {
            foreach (IMyPistonBase piston in pistons)
            {
                piston.Velocity = 0;
            }
        }

        private void initiateRotatingState()
        {
            stopAllPistons();
            rotor.TargetVelocityRPM = rotor.TargetVelocityRPM * -1;
            state = FlatteningState.Rotating;
        }

        private void initiateExtendingState()
        {
            activePiston = getFirstUnmaxedPiston();
            activePiston.Enabled = true;
            activePiston.Velocity = 0.1f;
            startingExtension = getPistonExtension();
        }

        private IMyPistonBase getFirstUnmaxedPiston()
        {
            foreach (IMyPistonBase piston in pistons)
            {
                if (piston.CurrentPosition < piston.MaxLimit)
                {
                    return piston;
                }
            }

            return null;
        }
    }
}
