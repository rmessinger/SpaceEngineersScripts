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
        struct AngleData
        {
            public AngleData(IMyMotorAdvancedRotor pivot, IMyMotorSuspension leftWheel, IMyMotorSuspension rightWheel)
            {
                this.pivot = pivot;
                this.leftWheel = leftWheel;
                this.rightWheel = rightWheel;
            }

            public IMyMotorAdvancedRotor pivot { get; }
            public IMyMotorSuspension leftWheel { get; }
            public IMyMotorSuspension rightWheel { get; }
        }

        IMyTextPanel diagDisplay = null;
        AngleData angleComponents;

        public Diagnostics()
        {
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (diagDisplay == null)
            {
                initDiagPanel();
            }

            outputDiagnostics();
        }

        private void outputDiagnostics()
        {
            // Matrix matrix;
            // diagDisplay.WriteText(matrix.ToString());
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

        private bool initAngleData()
        {
            angleComponents = new AngleData();
            IMyMotorSuspension testWheel;

            return true;
        }
    }
}
