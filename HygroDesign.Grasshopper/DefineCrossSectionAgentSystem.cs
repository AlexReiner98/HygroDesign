using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;

using HygroDesign.Core;
using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;

namespace HygroDesign.Grasshopper.Components
{
    public class DefineCrossSectionAgentSystem : GH_Component
    {
        public DefineCrossSectionAgentSystem()
          : base("Cross section agent system", "AgentSystem",
            "Define a cross section agent system from a cross section and a list of agents",
            "HygroDesign", "Design")
        {
        }

        CrossSectionAgentSystem agentSystem = null;
        List<CrossSectionAgent> agents = new List<CrossSectionAgent>();
        CrossSection thisCrossSection = null;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "The initial cross section for this agent system", GH_ParamAccess.item);
            pManager.AddGenericParameter("Agents", "A", "The cross section agents", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacement Threshold", "T", "The threshold after which agent system ends", GH_ParamAccess.item,-1.0);
            pManager.AddIntegerParameter("Soft Reset Value", "R", "Number of iterations between soft resets", GH_ParamAccess.item, -1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Agent System", "AS", "The cross section agent system", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrossSection crossSection = null;
            DA.GetData("Cross Section", ref crossSection);

            List<CrossSectionAgent> iAgents = new List<CrossSectionAgent>();
            DA.GetDataList("Agents", iAgents);

            double iDisplacementThreshold = 0;
            DA.GetData("Displacement Threshold", ref iDisplacementThreshold);

            int softReset = 0;
            DA.GetData("Soft Reset Value", ref softReset);

            // check if agents changed
            bool agentsChanged = false;
            if (agents.Count != iAgents.Count)
            {
                agentsChanged = true;
            }
            else
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    if (agents[i] != iAgents[i])
                    {
                        agentsChanged = true;
                        break;
                    }
                }
            }
            agents = iAgents;

            //check if cross section changed
            bool crossSectionChanged = false;
            if (agentSystem == null || crossSection != thisCrossSection) crossSectionChanged = true;
            thisCrossSection = crossSection;

            if (agentsChanged | crossSectionChanged) agentSystem = new CrossSectionAgentSystem(crossSection, agents);
            agentSystem.SoftResetIterations = softReset;

            agentSystem.DisplacementThreshold = iDisplacementThreshold;

            DA.SetData("Agent System", agentSystem);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("04DEDDC1-C64C-497A-B06C-FB12AAAF173B");
    }
}