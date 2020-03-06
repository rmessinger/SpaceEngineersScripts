using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineers.Utilities
{
    public class CruiseControl : MyGridProgram
    {
        ISet<IMyThrust> forwardThrusters = new HashSet<IMyThrust>();
        ISet<IMyThrust> reverseThrusters = new HashSet<IMyThrust>();
        ISet<IMyShipConnector> shipConnectors = new HashSet<IMyShipConnector>();
        IMyCubeGrid shipGrid;
        IMyCockpit shipCockpit;

        System.DateTime lastTime;
        Vector3D lastPosition;
        float cruiseSpeed;
        float minSpeed;
        bool enabled;

        public CruiseControl()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            shipGrid = Me.CubeGrid;
            cruiseSpeed = 105;
            minSpeed = 40;
            enabled = false;

            List<IMyShipConnector> allConnectors = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(allConnectors);
            foreach(IMyShipConnector connector in allConnectors)
            {
                if (connector.CubeGrid.IsSameConstructAs(shipGrid))
                {
                    shipConnectors.Add(connector);
                }
            }

            List<IMyCockpit> allCockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(allCockpits);
            foreach(IMyCockpit cockpit in allCockpits)
            {
                if (cockpit.CubeGrid.IsSameConstructAs(shipGrid))
                {
                    this.shipCockpit = cockpit;
                    break;
                }
            }

            InitializeThrusters();
        }

        public void Main()
        {
            if (ConnectorsLocked(shipConnectors))
            {
                return;
            }

            DateTime now = DateTime.Now;
            Vector3D position = shipGrid.GetPosition();
            double velocity = CalculateVelocity(lastPosition, position, lastTime, now);
            StringBuilder displayText = new StringBuilder();

            lastTime = now;
            lastPosition = shipGrid.GetPosition();

            bool stop = false;

            float target = 0;

            if (shipCockpit.MoveIndicator.Z == 1)
            { 
                if (enabled == true)
                {
                    stop = true;
                }

                enabled = false;
            }
            else if (velocity > minSpeed && shipCockpit.MoveIndicator.Z == -1)
            {
                enabled = true;
            }
            else if (velocity < minSpeed)
            {
                if (enabled == true)
                {
                    stop = true;
                }

                enabled = false;
            }

            if (enabled)
            {
                DisableReverseThrusters();
                target = SetThrusters(velocity);
            }
            else if (stop)
            {
                ResetThrustOverride();
            }

            displayText.Append(enabled ? "Enabled\n" : "Disabled\n");
            displayText.Append($"X:{Math.Round(shipCockpit.MoveIndicator.X, 3)} ");
            displayText.Append($"Y:{Math.Round(shipCockpit.MoveIndicator.Y, 3)} ");
            displayText.Append($"Z:{Math.Round(shipCockpit.MoveIndicator.Z, 3)}\n");
            displayText.Append($"V:{ Math.Round(velocity, 3)}\n");
            displayText.Append($"T:{ Math.Round(target, 3)}");

            WriteToLCD("Debug Panel 1", displayText.ToString());
            WriteToLCD("Debug Panel 2", GetReverseStatus());
        }

        private void InitializeThrusters()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);

            foreach (IMyThrust thruster in thrusters)
            {
                if (thruster.CubeGrid.IsSameConstructAs(shipGrid))
                {
                    // Thrustdirection of backward means the ship will move forward
                    if (thruster.GridThrustDirection.Equals(VRageMath.Vector3I.Backward))
                    {
                        forwardThrusters.Add(thruster);
                    }
                    else if (thruster.GridThrustDirection.Equals(VRageMath.Vector3I.Forward))
                    {
                        // TODO filter hydrogen thrusters when in gravity well
                        reverseThrusters.Add(thruster);
                    }
                }
            }
        }

        private string GetReverseStatus()
        {
            int enabled = 0;
            int disabled = 0;

            foreach (IMyThrust thruster in reverseThrusters)
            {
                if (thruster.Enabled)
                {
                    enabled++;
                }
                else
                {
                    disabled++;
                }
            }

            return $"Enabled:{enabled}\nDisabled:{disabled}";
        }

        private void DisableReverseThrusters()
        {
            foreach (IMyThrust thruster in reverseThrusters)
            {
                thruster.Enabled = false;
            }
        }

        private float SetThrusters(double velocity)
        {
            // Stalls around 20% thrust - apparently the amount required to maintain 80ish m/s
            float thrustPercentage = 1 - ((float)velocity / cruiseSpeed);

            foreach(IMyThrust thruster in forwardThrusters)
            {
                thruster.ThrustOverridePercentage = thrustPercentage;
            }

            return thrustPercentage;
        }

        // TODO this needs to somehow be called on deactivation of block
        private void ResetThrustOverride()
        {
            foreach (IMyThrust thruster in forwardThrusters)
            {
                thruster.ThrustOverridePercentage = 0;
            }

            foreach(IMyThrust thruster in reverseThrusters)
            {
                thruster.Enabled = true;
            }
        }

        private bool ConnectorsLocked(ISet<IMyShipConnector> connectors)
        {
            foreach (IMyShipConnector connector in connectors)
            {
                if (connector.Status != MyShipConnectorStatus.Connected)
                {
                    return false;
                }
            }

            return true;
        }

        private double CalculateVelocity(Vector3D startPosition, Vector3D endPosition, DateTime startTime, DateTime endTime)
        {
            double elapsed = (endTime - startTime).TotalSeconds;
            double xDistance = endPosition.X - startPosition.X;
            double yDistance = endPosition.Y - startPosition.Y;
            double zDistance = endPosition.Z - startPosition.Z;
            // pow!
            double distance = Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2) + Math.Pow(zDistance, 2));

            return distance / elapsed;
        }

        private void WriteToLCD(string panelName, string message)
        {
            IMyTextPanel panel;
            panel = GridTerminalSystem.GetBlockWithName(panelName) as IMyTextPanel;
            panel.ContentType = ContentType.TEXT_AND_IMAGE;

            // 0f is black
            panel.BackgroundColor = new Color(0f);
            panel.WriteText(message);
        }
    }
}
