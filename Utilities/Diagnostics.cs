using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace Utilities
{
    public class Diagnostics : MyGridProgram
    {
        IMyMotorAdvancedStator pivot = null;
        IMyMotorSuspension leftWheel = null;
        IMyMotorSuspension rightWheel = null;
        IDictionary<IMyEntity, Vector3D> lastReportedPosition = null;

        IMyTextPanel diagDisplay = null;

        public Diagnostics()
        {
            Runtime.UpdateFrequency |= UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (diagDisplay == null)
            {
                initDiagPanel();
            }

            if (pivot == null)
            {
                initPivot();
            }

            if (leftWheel == null || rightWheel == null)
            {
                initWheels();
            }

            if (lastReportedPosition == null)
            {
                lastReportedPosition = new Dictionary<IMyEntity, Vector3D>();
            }

            StringBuilder output = new StringBuilder();
            output.Append(pivot.CustomName + "\n");
            output.Append(pivot.Angle + "\n\n");
            // output.Append("Mean wheel position\n");
            Vector3D meanWheelPosition = getMeanPosition(leftWheel, rightWheel);
            // output.Append(meanWheelPosition.ToString().Replace(' ', '\n') + "\n\n");
            output.Append("Distance to pivot\n");
            output.Append(Vector3D.Distance(pivot.GetPosition(), meanWheelPosition) + "\n\n");
            float meanHeight = (leftWheel.Height + rightWheel.Height) / 2;
            output.Append("Height: " + meanHeight + "\n\n");

            if (pivot.Angle < 0.3 || pivot.Angle > 6)
            {
                if (pivot.Angle > 0 && pivot.Angle < 3.14)
                {
                    leftWheel.Height -= .01f;
                    rightWheel.Height -= .01f;
                }
                else
                {
                    leftWheel.Height += .01f;
                    rightWheel.Height += .01f;
                }
            }
            // output.Append(leftWheel.CustomName + "\n");
            // output.Append(leftWheel.GetPosition().ToString().Replace(' ', '\n') + "\n\n");
            // output.Append(rightWheel.CustomName + "\n");
            // output.Append(rightWheel.GetPosition().ToString().Replace(' ', '\n') + "\n\n");
            outputDiagnostics(output.ToString());
        }

        private void outputDiagnostics(string text)
        {
            diagDisplay.WriteText(text);
        }

        private bool initDiagPanel()
        {
            bool found = false;
            List<IMyTextPanel> allPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(allPanels);
            foreach (IMyTextPanel panel in allPanels)
            {
                if (panel.CustomName.Contains("Diagnostics Panel 1"))
                {
                    Echo(panel.CustomName);
                    diagDisplay = panel;
                    diagDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
                    diagDisplay.BackgroundColor = new Color(0f);
                    found = true;
                }
            }

            return found;
        }

        private bool initPivot()
        {
            IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName("Balancer Rotor 1");
            IMyMotorAdvancedStator rotorStatus = block as IMyMotorAdvancedStator;

            if (rotorStatus == null)
            {
                return false;
            }
            else
            {
                this.pivot = rotorStatus;
                return true;
            }
        }

        private bool initWheels()
        {
            IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName("Balancer Wheel Left");
            IMyMotorSuspension leftWheel  = block as IMyMotorSuspension;
            block = GridTerminalSystem.GetBlockWithName("Balancer Wheel Right");
            IMyMotorSuspension rightWheel = block as IMyMotorSuspension;

            if (leftWheel == null || rightWheel == null)
            {
                return false;
            }
            else
            {
                this.leftWheel = leftWheel;
                this.rightWheel = rightWheel;
                return true;
            }
        }

        private Vector3D getMeanPosition(IMyEntity entity1, IMyEntity entity2)
        {
            Vector3D position1 = entity1.GetPosition();
            Vector3D position2 = entity2.GetPosition();
            return new Vector3D((position1.X + position2.X) / 2, (position1.Y + position2.Y) / 2, (position1.Z + position2.Z) / 2);
        }
    }
}
