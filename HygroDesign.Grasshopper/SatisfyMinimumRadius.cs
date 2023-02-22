using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using HygroDesign.Core;
using ABxM.Core;

namespace HygroDesign.Grasshopper.Components
{

    public class SatisfyMinimumRadius : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SatisfyMinimumRadius()
          : base("Satisfy Minimum Radius", 
                "MinRad",
                "Update cross section to conform with minimum radius requirement.",
                "HygroDesign", 
                "Design")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "C", "The cross section", GH_ParamAccess.item);
            pManager.AddNumberParameter("Minimum Radius", "R", "Minimum radius for this bilayer cross section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cross Section", "C", "The updated cross section with minimum radius", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrossSection iCrossSection = null;
            double iMinRadius = 0.0;

            if (!DA.GetData(0,ref iCrossSection)) return;
            if (!DA.GetData(1, ref iMinRadius)) return;

            CrossSection thisCrossSection = new CrossSection(iCrossSection);
            thisCrossSection.SatisfyMinimumRadius(iMinRadius);

            DA.SetData(0, thisCrossSection);
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
        public override Guid ComponentGuid => new Guid("8DD9069E-46A2-4108-BF0A-477442AA9EE6");
    }
}