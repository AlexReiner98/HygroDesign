using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;
using System.Linq;

namespace HygroDesign.Grasshopper.Update
{

    public class DefinePanelSurface : GH_Component
    {

        public DefinePanelSurface()
          : base("Panel Surface", "Pan Srf",
            "Update a panel's underlying surface after curvature prediction.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to update.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Surface", "S", "The surface to update the panel with.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel oldpanel = null;
            Surface surface = null;
            DA.GetData(0, ref oldpanel);
            DA.GetData(1, ref surface);
            Panel panel = Panel.DeepCopy(oldpanel);
            panel.ShapedSurface = surface;
            DA.SetData(0, panel);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("D0AF5FFB-916D-4D0C-B85E-92D4E682A62D");
    }
}