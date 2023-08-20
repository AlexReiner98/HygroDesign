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

    public class DefineSelectStockBoards : GH_Component
    {
        public DefineSelectStockBoards()
          : base("Apply Stock", "App Stock",
            "Apply stock to panels using StockBoard.PotentialRadii dictionaries.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "Desired panel designs.", GH_ParamAccess.list);
            pManager.AddGenericParameter("StockPile", "SP", "The stockpile used in this design.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The updated panels with stock assigned.", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            StockPile stockPile = null;
            DA.GetData(1, ref stockPile);

            

            ApplicationEnvironment applicationEnvironment = new ApplicationEnvironment(panels, StockPile.DeepCopy(stockPile));

            DA.SetDataList(0, applicationEnvironment.Panels);
        }
        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("2B2A976E-AC53-4B2C-AB68-5379471C3AA7");
    }
}