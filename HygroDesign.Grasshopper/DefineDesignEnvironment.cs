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

    public class DefineDesignEnvironment : GH_Component
    {
        
        public DefineDesignEnvironment()
          : base("Design Environment", "Design Env",
            "Environment for combining stockpile and panel goals in order to generate final panel layouts.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "Desired panel designs.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stockpiles", "S", "The set of stock boards used in this design.", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The updated panels with stock assigned.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stockpiles", "S", "The updated stockpiles with fabrication information.", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            List<StockPile> stockpiles = new List<StockPile>();
            DA.GetDataList(1, stockpiles);

            DesignEnvironment designEnvironment = new DesignEnvironment(panels, stockpiles);

            DA.SetDataList(0, designEnvironment.Panels);
            DA.SetDataList(1, designEnvironment.StockPiles);
        }
        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("67D70D80-DAFB-471E-A733-0468771F9CA7");
    }
}