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

    public class DefineDeconstructActiveBoard : GH_Component
    {

        public DefineDeconstructActiveBoard()
          : base("Deconstruct Active Board", "Dec Active Board",
            "Deconstruct an active layer board into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Active Board", "A", "The active layer board to deconstruct.", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxels", "HM", "The active board's HMaxels.", GH_ParamAccess.list);
            pManager.AddGenericParameter("StockBoard", "S", "The active board's StockBoard.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Desired Radius", "DR", "The active board's desired radius.", GH_ParamAccess.item);
            //more properties need to be added here after prediction and board selection are updated
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ActiveBoard activeBoard = null;
            DA.GetData(0, ref activeBoard);

            DA.SetData(0, activeBoard.HMaxels);
            DA.SetData(1, activeBoard.StockBoard);
            DA.SetData(2, activeBoard.DesiredRadius);

        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E6A32054-86E6-4F37-A7E3-15F1636E1EEC");
    }
}