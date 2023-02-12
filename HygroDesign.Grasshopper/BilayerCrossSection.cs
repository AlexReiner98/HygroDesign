using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using HygroDesign.Core;

namespace HygroDesign.Grasshopper.Components
{

    public class BilayerCrossSection : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BilayerCrossSection()
          : base("Bilayer Cross Section", "CroSec",
            "Generate bilayer cross section curve.",
            "HygroDesign", "Design")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Radii", "R", "List of radii for cross section", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Directions", "D", "List of curvature directions for cross section", GH_ParamAccess.list);
            pManager.AddNumberParameter("Board Width", "W", "Board width used in composite", GH_ParamAccess.item, 0.15);
            pManager.AddNumberParameter("Snap Tolerance", "T", "Maximum distance to snap to closed cross section", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Support Parameters", "S", "Parameters along cross section to re-orient to the base plane", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Base Plane", "P", "Base plane for cross seciton", GH_ParamAccess.item, Plane.WorldZX);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddArcParameter("Board Curves", "B", "Board cross section curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> radii = new List<double>();
            List<int> directions = new List<int>();
            double boardWidth = 0.0;
            double snapTolerance = 0.0;
            List<double> supportParameters = new List<double>();
            Plane basePlane = Plane.Unset;

            if (!DA.GetDataList(0, radii)) return;
            if (!DA.GetDataList(1, directions)) return;
            if (!DA.GetData(2, ref boardWidth)) return;
            if (!DA.GetData(3, ref snapTolerance)) return;
            if (!DA.GetDataList(4, supportParameters)) return;
            if (!DA.GetData(5, ref basePlane)) return;

            CrossSection crossSection = new CrossSection(radii, directions, boardWidth, snapTolerance, supportParameters, basePlane);
        


            DA.SetDataList(0, crossSection.Arcs);
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
        public override Guid ComponentGuid => new Guid("69D66242-48CC-45A4-8BCA-B448AECB0F2E");
    }
}