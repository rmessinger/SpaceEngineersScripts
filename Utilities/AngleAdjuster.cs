using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace Utilities
{
    public class AngleAdjuster : MyGridProgram
    {
        IMyTextSurface panel = null;
        List<IMyMotorAdvancedStator> angleHinges = null;
        int numHinges = 3;
        string hingeKey = "Angle Hinge";
        string entityKey = "Rld";
        float radiansPerDegree = 0.0174533f;

        public AngleAdjuster()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Once;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            IList<string> splitargs = argument.Split().Select(s => s.Trim()).ToList();

            if (panel == null)
            {
                IMyCockpit seat = (IMyCockpit)GridTerminalSystem.GetBlockWithName("Rld Control Seat");
                initDiagPanel(seat.GetSurface(0));
            }

            if (angleHinges == null)
            {
                initHinges();
            }

            if (angleHinges.Count > 0)
            {
                StringBuilder output = new StringBuilder();
                int count = 0;
                angleHinges.ForEach(i => 
                {
                    output.Append(i.CustomName + ": (" + i.Angle / radiansPerDegree + ") ");
                    count++;
                    if (count < angleHinges.Count)
                    {
                        output.Append("\n");
                    }
                });
                panel.WriteText(output.ToString());
            }
        }

        private void initHinges()
        {
            List<IMyMotorAdvancedStator> hinges = new List<IMyMotorAdvancedStator>();
            angleHinges = new List<IMyMotorAdvancedStator>();
            GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedStator>(hinges);
            if (hinges == null && hinges.Count == 0)
            {
                Echo("No hinges found");
                return;
            }

            foreach (var hinge in hinges)
            {
                if (hinge.CustomName.Contains(entityKey) && hinge.CustomName.Contains(hingeKey))
                {
                    hinge.LowerLimitDeg = 0;
                    angleHinges.Add(hinge);
                }
            }
        }

        private void initDiagPanel(IMyTextSurface panel)
        {
            this.panel = panel;
            this.panel.ContentType = ContentType.TEXT_AND_IMAGE;
            this.panel.FontSize = 2;
            this.panel.Alignment = TextAlignment.CENTER;
            this.panel.BackgroundColor = new Color(0f);
        }
    }
}
