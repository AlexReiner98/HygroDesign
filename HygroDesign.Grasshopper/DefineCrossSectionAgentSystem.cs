using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
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

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "CS", "The initial cross section for this agent system", GH_ParamAccess.item);
            pManager.AddGenericParameter("Agents", "A", "The cross section agents", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Agent System", "AS", "The cross section agent system", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrossSection crossSection = null;
            DA.GetData("Cross Section", ref crossSection);

            List<CrossSectionAgent> iAgents = new List<CrossSectionAgent>();
            DA.GetDataList("Agents", iAgents);

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

            if(agentsChanged)
                agentSystem = new CrossSectionAgentSystem(crossSection, agents);

            DA.SetData("Agent System", agentSystem);
        }


        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("04DEDDC1-C64C-497A-B06C-FB12AAAF173B");
    }
}