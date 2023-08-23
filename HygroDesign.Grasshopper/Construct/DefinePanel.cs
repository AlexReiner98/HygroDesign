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

    public class DefinePanel : GH_Component
    {

        public DefinePanel()
          : base("Panel", "Panel",
            "Create a new self shaping panel.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("HMaxel Length", "HL", "The length of each HMaxel.", GH_ParamAccess.item);
            pManager.AddNumberParameter("HMaxel Width,", "HW", "The width of each HMaxel.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Length Count", "LC", "The number of HMaxels in the length direction.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width Count", "WC", "The number of HMaxels in the width direction.", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double hmaxelLength = 0;
            double hmaxelWidth = 0;
            int lengthCount = 0;
            int widthCount = 0;

            DA.GetData(0, ref hmaxelLength);
            DA.GetData(1, ref hmaxelWidth);
            DA.GetData(2, ref lengthCount);
            DA.GetData(3, ref widthCount);

            Panel panel = new Panel(hmaxelLength, hmaxelWidth, lengthCount, widthCount);

            DA.SetData(0, panel);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("FD243E1E-8030-43CF-B5D5-03D925DE3E2A");
    }
}