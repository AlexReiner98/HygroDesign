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

    public class DefineDeconstructStockBoard : GH_Component
    {

        public DefineDeconstructStockBoard()
          : base("Deconstruct Stock Board", "Dec Stock Board",
            "Deconstruct an stock board layer board into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stock Board", "S", "The stock board to deconstruct.", GH_ParamAccess.tree);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The stock board's name.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Species", "S", "The stock board's species.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("RT Angle", "RT", "The stock board's RT angle.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Length", "L", "The stock board's total length.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Width", "W", "The stock board's total width.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Active Boards,", "AB", "The list of active boards that use this stock board.", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTree<string> names = new DataTree<string>();
            DataTree<Species> species = new DataTree<Species>();
            DataTree<double> rts = new DataTree<double>();
            DataTree<double> lengths = new DataTree<double>();
            DataTree<double> widths = new DataTree<double>();
            DataTree<ActiveBoard> activeBoards = new DataTree<ActiveBoard>();

            GH_Structure<IGH_Goo> stockBoards = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out stockBoards);
            for(int i = 0; i< stockBoards.Branches.Count; i++)
            {
                for(int j = 0; j < stockBoards.Branches[i].Count; j++)
                {
                    StockBoard stockBoard = null;
                    stockBoards.Branches[i][j].CastTo<StockBoard>(out stockBoard);

                    GH_Path path = stockBoards.Paths[i].AppendElement(j);
                    names.Add(stockBoard.Name, path);
                    species.Add(stockBoard.Species, path);
                    rts.Add(stockBoard.RTAngle, path);
                    lengths.Add(stockBoard.Length, path);
                    widths.Add(stockBoard.Width, path);
                    activeBoards.AddRange(stockBoard.DesignBoards, path);
                }
            }

            DA.SetDataTree(0, names);
            DA.SetDataTree(1, species);
            DA.SetDataTree(2, rts);
            DA.SetDataTree(3, lengths);
            DA.SetDataTree(4, widths);
            DA.SetDataTree(5, activeBoards);


        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("CC0CCB72-65AE-4E14-9538-4FD9E9CEF866");
    }
}