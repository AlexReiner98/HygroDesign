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

    public class DefineDeconstructHMaxel : GH_Component
    {

        public DefineDeconstructHMaxel()
          : base("Deconstruct HMaxel", "Dec HMaxel",
            "Deconstruct an HMaxel into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxel", "HM", "The HMaxel to deconstruct.", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Passive Layers", "P", "The HMaxel's passive layers.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Active Boards", "A", "The HMaxel's active layer boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Height", "T", "The HMaxel's height.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Row Range", "R", "The HMaxel's length interval.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Column Range", "C", "The HMaxel's width interval.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            HMaxel hmaxel = null;
            DA.GetData(0, ref hmaxel);

            DA.SetDataList(0, hmaxel.PassiveLayers);
            DA.SetDataList(1, hmaxel.ActiveBoards);
            DA.SetData(2, hmaxel.Height);
            DA.SetData(3, hmaxel.RowRange);
            DA.SetData(4, hmaxel.ColumnRange);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("F2E853BB-E524-4777-A6F0-9E7EF19BC497");
    }
}