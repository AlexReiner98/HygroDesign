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

    public class DefineRadiusPrediction : GH_Component
    {

        public DefineRadiusPrediction()
          : base("Radius Prediction", "Pred Rad",
            "Predict the radii for each board in the panel.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to predict the curvature of.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Prediction Engine", "PE", "The prediction engine..", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            PredictionEngine predictionEngine = null;
            DA.GetData(1, ref predictionEngine);

            for(int i = 0; i < panel.Bilayers.Count; i++)
            {
                for(int j = 0; j < panel.Bilayers[i].ActiveLayer.Boards.Count; j++)
                {
                    panel.Bilayers[i].ActiveLayer.Boards[j].Radius = predictionEngine.Predict(panel.Bilayers[i].ActiveLayer.Boards[j].StockBoard, panel.Bilayers[i], panel.Bilayers[i].ActiveLayer.Boards[j].StockBoard.MoistureChange);
                }
            }

            DA.SetData(0, panel);
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("5CCA5E70-60AC-48BA-9084-92A906116CCB");
    }
}