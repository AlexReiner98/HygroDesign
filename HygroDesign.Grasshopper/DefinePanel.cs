using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefinePanel : GH_Component
    {
        
        public DefinePanel()
          : base("Panel", "Panel",
            "Generate Panel",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayers", "B", "The list of bilayers from which to create the panel", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel object", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var bilayers = new List<Bilayer>();
            DA.GetDataList(0, bilayers);

            Panel panel = new Panel(bilayers);

            DA.SetData(0, panel);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("9B4D795A-DB75-4445-A3D9-ABCF8D2F3691");
    }
}