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

        enum RunMode
        {
            None,
            Flattening,
            Diagnostics
        }

        IMyMotorAdvancedStator rotor = null;
        ISet<IMyPistonBase> pistons = new HashSet<IMyPistonBase>();
        ISet<IMyShipDrill> drills = new HashSet<IMyShipDrill>();
        IMyPistonBase activePiston = null;
        IMyPistonBase flattenerPistonPrime = null;
        IMyTextPanel diagDisplay = null;

        FlatteningState state = FlatteningState.Unknown;
        RunMode mode = RunMode.None;

        float minAngle = 3.147f;
        float maxAngle = 6.26573f;
        float startingExtension = 0;

        // extend pistons by 1 meter each time rotor reaches limit
        float extensionPerCycle = 1.0f;

        public Flattener()
        {
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            bool allComponentsInitialized = initComponents();
            if (argument.Contains("flatten") && allComponentsInitialized)
            {
                mode = RunMode.Flattening;
            }

            if (argument.Contains("diag"))
            {
                if (diagDisplay == null && !initDiagPanel())
                {
                    Echo("No diag panel found");
                    return;
                }

                mode = RunMode.Diagnostics;
            }

            if (mode == RunMode.Flattening)
            {
                flatten();
            }
            else if (mode == RunMode.Diagnostics)
            {
                outputDiagnostics();
            }
        }

        private void outputDiagnostics()
        {
            Matrix matrix;
            flattenerPistonPrime.Orientation.GetMatrix(out matrix);
            diagDisplay.WriteText(matrix.ToString());
        }

        private bool initDiagPanel()
        {
            bool found = false;
            List<IMyTextPanel> allPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(allPanels);
            foreach (IMyTextPanel panel in allPanels)
            {
                if (panel.CustomName.Contains("Flattener Diag"))
                {
                    diagDisplay = panel;
                    diagDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
                    diagDisplay.BackgroundColor = new Color(0f);
                    found = true;
                }
            }

            return found;
        }

        private bool initComponents()
        {
            bool allFound = true;
            if (rotor == null && !findRotor())
            {
                Echo("No rotor found");
                allFound = false;
            }

            if ((pistons == null || pistons.Count == 0) && !findPistons())
            {
                Echo("No pistons found");
                allFound = false;
            }

            if ((drills == null || drills.Count == 0) && !findDrills())
            {
                Echo("No drills found");
            }

            return allFound;
        }

        private void flatten()
        {
            // Is the motor in action?
            if (rotor.RotorLock || !rotor.IsWorking || rotor.TargetVelocityRPM == 0)
            {
                Echo("ROTOR SEEMS DISABLED I GUESS?!");
            }

            // If state is unknown, well, HOW DID I GET HERE
            // AND THE DAYS GO BY
            if (state == FlatteningState.Unknown)
            {
                Echo("State is unknown");
                // determine what angle the rotor is turning
                // positive vel rpm means it's returning to home
                // hey, don't be ignorant. Negative vel can mean home too
                if (rotor.Angle > minAngle && rotor.Angle < maxAngle)
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
                    initiateRotatingState();
                }

            }
            else if (state == FlatteningState.Rotating)
            {
                Echo("Rotating: angle = " + rotor.Angle);
                // if the rotor has reached the target angle, start extending the pistons
                // TODO: Only initiate extending state if the rotor is still rotating toward the reached destination
                if ((rotor.Angle >= maxAngle && rotor.TargetVelocityRPM > 0)
                    || (rotor.Angle <= minAngle && rotor.TargetVelocityRPM < 0))
                {
                    Echo("Reached target");
                    initiateExtendingState();
                }
            }
            else if (state == FlatteningState.Extending)
            {
                Echo("Extending: " + getPistonExtension());
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

            if (rotorStatus == null)
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
            List<IMyPistonBase> allPistons = new List<IMyPistonBase>();
            bool ret = false;
            this.GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(allPistons);

            if (allPistons == null || allPistons.Count == 0)
            {
                return false;
            }

            foreach(IMyPistonBase piston in allPistons)
            {
                if (piston.CustomName.Contains("Flattener"))
                {
                    pistons.Add(piston);
                    Echo("Found piston " + piston.CustomName);

                    ret = true;
                    if (flattenerPistonPrime == null && piston.CustomName.Contains("Flattener Piston Prime"))
                    {
                        flattenerPistonPrime = piston;
                    }
                }
            }

            return ret;
        }

        private bool findDrills()
        {
            List<IMyShipDrill> allDrills = new List<IMyShipDrill>();
            bool ret = false;
            this.GridTerminalSystem.GetBlocksOfType(allDrills);

            foreach(IMyShipDrill drill in allDrills)
            {
                if (!Me.CubeGrid.IsSameConstructAs(drill.CubeGrid))
                {
                    continue;
                }
                else if (drill.Name.Contains("Flattener"))
                {
                    drills.Add(drill);
                    ret = true;
                }
            }

            return ret;
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
            state = FlatteningState.Extending;
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
