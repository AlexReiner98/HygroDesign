using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Deconstruct
{

    public class DefineDeconstructPassiveLayer : GH_Component
    {

        public DefineDeconstructPassiveLayer()
          : base("Deconstruct Passive Layer", "Dec Passive",
            "Deconstruct a passive layer into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Passive Layer", "P", "The passive layer to deconstruct.", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Species", "S", "The passive layer's species.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thickness", "T", "The passive layer's thickness.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PassiveLayer pLayer = null;
            DA.GetData(0, ref pLayer);

            DA.SetData(0, pLayer.Species);
            DA.SetData(1, pLayer.Thickness);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("346ACC96-A8BD-411C-8081-B797705E34F4");
    }
}