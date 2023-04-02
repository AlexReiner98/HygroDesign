using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using HygroDesign.Core;
using ABxM.Core.Behavior;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineCrossSectionAgent : GH_Component
    {

        List<CrossSectionAgent> agents = new List<CrossSectionAgent>();
        List<BehaviorBase> behaviors = new List<BehaviorBase>();

        public DefineCrossSectionAgent()
          : base("Cross section agent", "Agent",
            "Define a cross section agent from a starting point and a list of behaviours",
            "HygroDesign", "Design")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Starting Point", "P", "The starting point for the agent", GH_ParamAccess.list);
            pManager.AddGenericParameter("Behaviours", "B", "The bahaviours of this agent", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section Agent", "A", "The cross section agent", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> iPositions = new List<Point3d>();
            DA.GetDataList("Starting Point", iPositions);

            List<BehaviorBase> iBehaviors = new List<BehaviorBase>();
            DA.GetDataList("Behaviours", iBehaviors);

            // check if positions changed
            bool positionsChanged = false;
            if (iPositions.Count != agents.Count)
            {
                positionsChanged = true;
            }
            else
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    if (agents[i].StartPosition != iPositions[i])
                    {
                        positionsChanged = true;
                        break;
                    }
                }
            }

            // check if behaviours changed
            bool behaviorsChanged = false;
            if (iBehaviors.Count != behaviors.Count)
            {
                behaviorsChanged = true;
            }
            else
            {
                for (int i = 0; i < iBehaviors.Count; i++)
                {
                    if (iBehaviors[i] != behaviors[i])
                    {
                        behaviorsChanged = true;
                    }
                }
            }
            behaviors = iBehaviors;

            // create agents
            if (positionsChanged)
            {
                agents = new List<CrossSectionAgent>();
                for (int i = 0; i < iPositions.Count; i++)
                {
                    Point3d thisPosition = iPositions[i];
                    agents.Add(new CrossSectionAgent(thisPosition, iBehaviors));
                }
            }

            // update behaviours
            if (behaviorsChanged)
            {
                foreach (CrossSectionAgent ca in agents)
                {
                    ca.Behaviors = iBehaviors;
                }
            }

            DA.SetDataList("Cross Section Agent", agents);
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
        public override Guid ComponentGuid => new Guid("1F6C7511-B322-4B87-B018-DE65B588CCF9");
    }
}