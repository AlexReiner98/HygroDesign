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

    public class DefineDeconstructBilayer : GH_Component
    {

        public DefineDeconstructBilayer()
          : base("Deconstruct Bilayer", "Dec Bilayer",
            "Deconstruct a bilayer into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "B", "The bilayer to deconstruct.", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Active Layer", "A", "The bilayer's active layer.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Passive Layer", "P", "The bilayer's poassive layer.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bilayer bilayer = null;
            DA.GetData(0, ref bilayer);

            DA.SetData(0, bilayer.ActiveLayer);
            DA.SetData(1, bilayer.PassiveLayer);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("C61A1653-81F0-48E9-823A-5F948827C91C");
    }
}