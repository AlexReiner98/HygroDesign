using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;

using HygroDesign.Core.DQL;


namespace HygroDesign.Core
{
    public class CrossSectionAgentSystem : CartesianAgentSystem
    {
        public CrossSection CrossSection;
        public double TotalDisplacement = double.MaxValue;
        public double DisplacementThreshold = -1.0;
        public Trainer Trainer = new Trainer();

        public CrossSectionAgentSystem(CrossSection crossSection, List<CrossSectionAgent> agents)
        {
            CrossSection = crossSection;

            Agents = new List<AgentBase>();
            for (int index = 0; index < agents.Count; ++index)
            {
                agents[index].CrossSectionAgentSystem = this;
                agents[index].Id = index;
                agents[index].AgentSystem = this;
                Agents.Add(agents[index]);
            }
        }

        public override void Reset()
        {
            base.Reset();
            UpdateCrossSection();
            CrossSection.NurbsToBoardCurves();
            TotalDisplacement = double.MaxValue;
        }

        public override void PreExecute()
        {
            base.PreExecute();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void PostExecute()
        {
            TotalDisplacement = 0.0;

            base.PostExecute();
            UpdateCrossSection();
            CrossSection.NurbsToBoardCurves();

        }

        public override bool IsFinished() => base.IsFinished();

        public void UpdateCrossSection()
        {
            for(int i = 0; i < CrossSection.NurbsCurve.Points.Count; i++)
            {
                CrossSectionAgent agent = (CrossSectionAgent)Agents[i];
                CrossSection.NurbsCurve.Points.SetPoint(i,agent.Position);
            }
        }
    }
}