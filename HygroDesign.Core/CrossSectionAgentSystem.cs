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
using ABxM.Core.Behavior;

namespace HygroDesign.Core
{
    public class CrossSectionAgentSystem : CartesianAgentSystem
    {
        public CrossSection CrossSection;
        public double TotalDisplacement = double.MaxValue;
        public double DisplacementThreshold = -1.0;
        public int Iterations = 0;

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
            Iterations = 0;
        }

        //reset everything except for the DQL models
        //used to reset the states of the agent iteratively during training
        public void SoftReset()
        {
            foreach (CrossSectionAgent agent in Agents) agent.SoftReset();
            UpdateCrossSection();
            CrossSection.NurbsToBoardCurves();
            TotalDisplacement = double.MaxValue;
            Iterations = 0;
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
            Iterations++;

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