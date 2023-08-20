using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineRadiusPrediction : GH_Component
    {
        
        public DefineRadiusPrediction()
          : base("Radius Prediction", "Rad Pred",
            "Predicts radii for each stock board in all possible bilayer permutations.(Updates StockBoard.PotentialRadii dictionary)",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "Desired panel designs.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stock Boards", "S", "The set of stock boards used in this design.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Moisture Changes", "M", "The set of moisture changes used in this design", GH_ParamAccess.list);
            pManager.AddGenericParameter("Prediction Engine", "E", "The engine used to predict self shaping radius.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StockPile", "SP", "The stockpile complete with self shaping radius predictions", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);
            

            List<StockBoard> stockBoards = new List<StockBoard>();
            DA.GetDataList(1, stockBoards);

            List<double> moistureChanges = new List<double>();
            DA.GetDataList(2, moistureChanges);

            PredictionEngine predictionBase = null;
            DA.GetData(3, ref predictionBase);

            StockPile stockPile = new StockPile(panels, stockBoards, moistureChanges, predictionBase);

            DA.SetData(0, stockPile);
        }
        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("67D70D80-DAFB-471E-A733-0468771F9CA7");
    }
}