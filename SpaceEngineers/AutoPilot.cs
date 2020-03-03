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
        IMyShipConnector connector;
        VRage.Game.ModAPI.Ingame.IMyCubeGrid grid;
        long lastTime;
        Vector3D lastPosition;

        public AutoPilot()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            connector = GridTerminalSystem.GetBlockWithName("MM Connector") as IMyShipConnector;
            grid = connector.CubeGrid;
        }

        public void Main()
        {
            if (connector.Status == MyShipConnectorStatus.Connected)
            {
                // return;
            }
            else if (controlledThrusters.Count == 0)
            {
                // InitializeThrusters();
            }

            lastPosition = grid.GetPosition();
            StringBuilder pos = new StringBuilder();
            pos.Append($"X: {lastPosition.X}\nY: {lastPosition.Y}\nZ: {lastPosition.Z}");
            WriteToLCD(pos.ToString());
        }

        public void InitializeThrusters()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);

            foreach (IMyThrust thruster in thrusters)
            {
                // Echo(thruster.GridThrustDirection.Equals(VRageMath.Vector3I.Forward).ToString());
                if (thruster.GridThrustDirection.Equals(VRageMath.Vector3I.Backward)
                    && thruster.CustomName.Contains("Large Atmospheric"))
                {
                    controlledThrusters.Add(thruster);
                    Echo(thruster.CustomName);
                }
            }
        }

        public void WriteToLCD(string message)
        {
            IMyTextSurface surface;
            surface = GridTerminalSystem.GetBlockWithName("MM Panel") as IMyTextSurface;
            surface.ContentType = ContentType.SCRIPT;
            using (var frame = surface.DrawFrame())
            {
                MySprite displayText = MySprite.CreateText(message, "Debug", new Color(1f), 2f, TextAlignment.CENTER);
                frame.Add(displayText);
            }
        }
    }
}
