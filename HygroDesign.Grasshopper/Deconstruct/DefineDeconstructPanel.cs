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

    public class DefineDeconstructPanel : GH_Component
    {

        public DefineDeconstructPanel()
          : base("Deconstruct Panel", "Dec Pan",
            "Deconstruct a panel into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to deconstruct.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayers", "B", "The panel's bilayers.", GH_ParamAccess.list);
            pManager.AddGenericParameter("HMaxels", "HM", "The list of HMaxles in the panel.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Length Range", "LR", "The interval describing the length of the panel.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Width Range", "WR", "The interval describing the width of the panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            List<Bilayer> bilayers = new List<Bilayer>();
            for(int i = 0; i < panel.Bilayers.Count; i++)
            {
                bilayers.Add(panel.Bilayers[i]);
            }
            DA.SetDataList(0,bilayers);
            List<HMaxel> hmaxels = new List<HMaxel>();
            for (int i = 0; i < panel.HMaxels.GetLength(0); i++)
            {
                for (int j = 0; j < panel.HMaxels.GetLength(1); j++)
                {
                    hmaxels.Add(panel.HMaxels[i, j]);
                }
            }
            DA.SetDataList(1, hmaxels);
            DA.SetData(2, new Interval(0,panel.Length));
            DA.SetData(3, new Interval(0,panel.Width));
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("6A332266-A3FB-46B0-8006-41CE4F737B94");
    }
}