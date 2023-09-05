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

    public class DefineSelectionEvironment : GH_Component
    {

        public DefineSelectionEvironment()
          : base("Construct Selection Environment", "Sel Env",
            "Create a new stock selection environment.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The panels the stock will be applied to.", GH_ParamAccess.list);
            pManager.AddGenericParameter("StockPile", "S", "The stockpile to apply.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The updated panels.", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            StockPile stockpile = null;
            DA.GetData(1, ref stockpile);

            SelectionEnvironment selectionEnvironment = new SelectionEnvironment(panels, stockpile);

            DA.SetDataList(0, selectionEnvironment.Panels);
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("7BB58230-2AEC-461E-B8A4-D20BA0BCF183");
    }
}