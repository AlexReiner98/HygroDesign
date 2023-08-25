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
            pManager.AddGenericParameter("Active Board", "A", "The active layer board to deconstruct.", GH_ParamAccess.tree);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxels", "HM", "The active board's HMaxels.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("StockBoard", "S", "The active board's StockBoard.", GH_ParamAccess.tree);
            pManager.AddIntervalParameter("Row Range", "R", "The active board's row range.", GH_ParamAccess.tree);
            pManager.AddIntervalParameter("Column Range", "C", "The active board's column range.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Desired Radius", "DR", "The active board's desired radius.", GH_ParamAccess.tree);
            //more properties need to be added here after prediction and board selection are updated
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTree<HMaxel> hmaxelTree = new DataTree<HMaxel>();
            DataTree<StockBoard> stockTree = new DataTree<StockBoard>();
            DataTree<double> drTree = new DataTree<double>();
            DataTree<Interval> rowRanges = new DataTree<Interval>();
            DataTree<Interval> columnRanges = new DataTree<Interval>();


            GH_Structure<IGH_Goo> activeBoards = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out activeBoards);
            for(int i = 0; i< activeBoards.Branches.Count; i++)
            {
                for(int j = 0; j < activeBoards.Branches[i].Count; j++)
                {
                    ActiveBoard activeBoard = null;
                    activeBoards.Branches[i][j].CastTo<ActiveBoard>(out activeBoard);

                    GH_Path path = activeBoards.Paths[i].AppendElement(j);
                    hmaxelTree.AddRange(activeBoard.HMaxels, path);
                    stockTree.Add(activeBoard.StockBoard, path);
                    drTree.Add(activeBoard.DesiredRadius, path);
                    rowRanges.Add(activeBoard.RowRange, path);
                    columnRanges.Add(activeBoard.ColumnRange, path);
                }
                
            }
            

            DA.SetDataTree(0, hmaxelTree);
            DA.SetDataTree(1, stockTree);
            DA.SetDataTree(2, rowRanges);
            DA.SetDataTree(3, columnRanges);
            DA.SetDataTree(4, drTree); 

        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E6A32054-86E6-4F37-A7E3-15F1636E1EEC");
    }
}