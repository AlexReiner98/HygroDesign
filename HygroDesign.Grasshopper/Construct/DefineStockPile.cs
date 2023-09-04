using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Construct
{

    public class DefineStockPile : GH_Component
    {

        public DefineStockPile()
          : base("Construct StockPile", "StockPile",
            "Create a new StockPile.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The panels the stock will be applied to.", GH_ParamAccess.list);
            pManager.AddGenericParameter("StockBoards", "S", "The stock boards to evaluate.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Moisture Changes", "MC", "The list of moisture changes to evaluate with.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Prediction Engine", "PE", "The prediction engine to evaluate with.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StockPile", "SP", "The StockPile.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            List<StockBoard> oldStockBoards = new List<StockBoard>();
            DA.GetDataList(1, oldStockBoards);
            List<StockBoard> stockBoards = new List<StockBoard>();
            for(int i = 0; i < oldStockBoards.Count; i++)
            {
                stockBoards.Add(StockBoard.DeepCopy(oldStockBoards[i]));
            }

            List<double> moistureChanges = new List<double>();
            DA.GetDataList(2, moistureChanges);

            PredictionEngine predictionEngine = null;
            DA.GetData(3, ref predictionEngine);

            StockPile stockpile = new StockPile(panels, stockBoards, moistureChanges, predictionEngine);

            DA.SetData(0, stockpile);
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("41560D84-0B93-4EC3-B076-1D6516FF54DD");
    }
}