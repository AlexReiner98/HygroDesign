using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineUpdatePanelSurface : GH_Component
    {
        
        public DefineUpdatePanelSurface()
          : base("Update Panel Surface", "Pan Srf",
            "Add a surface describing the panel's shaped curvature.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to update", GH_ParamAccess.item);
            pManager.AddGenericParameter("Surface", "S", "The surface to update with", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel object", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            Surface surface = null;
            DA.GetData(1, ref surface);

            panel.Surface = surface;
            panel.CalculateCenterOfGravity();

            DA.SetData(0, panel);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("0247921B-510A-48BF-A86C-ECD5998C8139");
    }
}