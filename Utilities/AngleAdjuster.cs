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
        string hingeKey = "Angle Hinge";
        string entityKey = "Rld";
        float radiansPerDegree = 0.0174533f;
        float epsilon = .2f;

        public AngleAdjuster()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
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
                bool allAtTarget = true;
                angleHinges.ForEach(i => 
                {
                    float angleDegrees = i.Angle / radiansPerDegree;
                    float angleTarget = 0;

                    if (i.TargetVelocityRPM > 0)
                    {
                        angleTarget = i.UpperLimitDeg;
                    }
                    else if (i.TargetVelocityRPM < 0)
                    {
                        angleTarget = i.LowerLimitDeg;
                    }


                    float delta = Math.Abs(angleDegrees - angleTarget);
                    if (delta > epsilon)
                    {
                        allAtTarget = false;
                    }

                    output.Append($"{i.CustomName} : {Math.Round(angleDegrees, 3)} / {Math.Round(angleTarget, 3)} - {delta}");
                    
                    count++;
                    if (count < angleHinges.Count)
                    {
                        output.Append("\n");
                    }
                });

                if (allAtTarget)
                {
                    angleHinges.ForEach(i =>
                    {
                        i.TargetVelocityRPM *= -1;
                    });
                }


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
