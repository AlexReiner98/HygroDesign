using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Update
{

    public class DefineUpdateHMaxel : GH_Component
    {

        public DefineUpdateHMaxel()
          : base("Update HMaxel", "Up HMaxel",
            "Update an HMaxel with its height.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxel", "HM", "The HMaxel to update.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "The maximum height allowed in this HMaxel.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxel", "HM", "The updated HMaxel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            HMaxel hmaxel = null;
            DA.GetData(0, ref hmaxel);

            double height = 0;
            DA.GetData(1, ref height);
            hmaxel.Height = height;
            DA.SetData(0, hmaxel);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("4827BDFB-38E8-4BE4-8A1A-D9115F4ABF4F");
    }
}