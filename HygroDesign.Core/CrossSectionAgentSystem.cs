﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;


namespace HygroDesign.Core
{
    public class CrossSectionAgentSystem : CartesianAgentSystem
    {
        public CrossSection CrossSection;

        public CrossSectionAgentSystem(CrossSection crossSection, List<CrossSectionAgent> agents)
        {
            CrossSection = crossSection;
            CrossSection.StartingNurbsCurve = CrossSection.NurbsCurve;

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
            CrossSection.NurbsCurve = CrossSection.StartingNurbsCurve;
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
            base.PostExecute();
            UpdateCrossSection();
            //CrossSection.NurbsToBoardCurves();
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