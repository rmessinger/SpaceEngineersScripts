using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Common;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRageMath;

namespace SpaceEngineers.Flight
{
    public class AutoPilot : MyGridProgram
    {
        List<IMyThrust> controlledThrusters = new List<IMyThrust>();
        List<IMyShipConnector> shipConnectors = new List<IMyShipConnector>();
        VRage.Game.ModAPI.Ingame.IMyCubeGrid shipGrid;
        IMyCockpit shipCockpit;

        System.DateTime lastTime;
        Vector3D lastPosition;
        float maxX, maxY;
        float cruiseSpeed;
        float minSpeed;
        bool enabled;

        public AutoPilot()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            shipGrid = Me.CubeGrid;
            cruiseSpeed = 105;
            minSpeed = 40;
            maxX = 0;
            maxY = 0;
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
        }

        public void Main()
        {
            if (ConnectorsLocked(shipConnectors))
            {
                // return;
            }
            else if (controlledThrusters.Count == 0)
            {
                InitializeThrusters();
            }

            DateTime now = DateTime.Now;
            Vector3D position = shipGrid.GetPosition();
            double velocity = CalculateVelocity(lastPosition, position, lastTime, now);
            StringBuilder displayText = new StringBuilder();

            lastTime = now;
            lastPosition = shipGrid.GetPosition();

            float xVel = shipCockpit.MoveIndicator.X;
            float yVel = shipCockpit.MoveIndicator.Y;
            bool stop = false;

            if (xVel < maxX)
            {
                maxX = xVel;
            }
            if (yVel < maxY)
            {
                maxY = yVel;
            }

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
                // WriteToLCD("Debug Panel 2", "Enabling", displayText.ToString());
                enabled = true;
            }
            else if (velocity < minSpeed)
            {
                enabled = false;
            }

            if (enabled)
            {
                Echo("Enabling");
                target = SetThrusters(velocity);
            }
            else if (stop)
            {
                ResetThrustOverride();
            }

            displayText.Append(enabled ? "Enabled\n" : "Disabled\n");
            displayText.Append($"X:{Math.Round(shipCockpit.MoveIndicator.X, 3)} ");
            displayText.Append($"Y:{Math.Round(shipCockpit.MoveIndicator.Y, 3)} ");
            displayText.Append($"Z:{Math.Round(shipCockpit.MoveIndicator.Z, 3)} ");
            displayText.Append($"\nV:{ Math.Round(velocity, 3)}");
            WriteToLCD("Debug Panel 1", displayText.ToString());
            Echo($"Enabled={enabled} Stop={stop} x={shipCockpit.MoveIndicator.X} y={shipCockpit.MoveIndicator.Y}");
            // displayText.Append($"X: {lastPosition.X}\nY: {lastPosition.Y}\nZ: {lastPosition.Z}");
        }

        public void InitializeThrusters()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);

            foreach (IMyThrust thruster in thrusters)
            {
                // Echo(thruster.GridThrustDirection.Equals(VRageMath.Vector3I.Forward).ToString());
                if (thruster.GridThrustDirection.Equals(VRageMath.Vector3I.Backward)
                    && thruster.CubeGrid.IsSameConstructAs(shipGrid))
                {
                    controlledThrusters.Add(thruster);
                    // Echo(thruster.CustomName);
                }
            }
        }

        public float SetThrusters(double velocity)
        {
            float thrustPercentage = 1 - (Math.Max(0, 85 - (float)velocity) / cruiseSpeed);

            foreach(IMyThrust thruster in controlledThrusters)
            {
                thruster.ThrustOverridePercentage = thrustPercentage;
            }

            return thrustPercentage;
        }

        public void ResetThrustOverride()
        {
            foreach (IMyThrust thruster in controlledThrusters)
            {
                thruster.ThrustOverridePercentage = 0;
            }
        }

        private bool ConnectorsLocked(List<IMyShipConnector> connectors)
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
            double distance = Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2) + Math.Pow(zDistance, 2));

            return distance / elapsed;
        }

        public void WriteToLCD(string panelName, string message)
        {
            IMyTextPanel panel;
            panel = GridTerminalSystem.GetBlockWithName(panelName) as IMyTextPanel;
            panel.ContentType = ContentType.TEXT_AND_IMAGE;
            panel.BackgroundColor = new Color(0f);
            panel.WriteText(message);
            // using (var frame = surface.DrawFrame())
            // {
            //     MySprite displayText = MySprite.CreateText(message, "Debug", new Color(1f), 2f, TextAlignment.LEFT);
            //     frame.Add(displayText);
            // }
        }
    }
}
