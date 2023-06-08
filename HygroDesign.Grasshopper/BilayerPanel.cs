using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class BilayerPanel : GH_Component
    {
        
        public BilayerPanel()
          : base("Bilayer Panel", "Panel",
            "Generate Bilayer Panel",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Base Plane", "P", "The base plane for the panel to be generated from", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Board Width", "W", "The width of a single board in the panel", GH_ParamAccess.item);
            pManager.AddGenericParameter("Board Length", "L", "The length of a single board in the panel", GH_ParamAccess.item);
            pManager.AddGenericParameter("Width Count", "WC", "The number of boards in the width", GH_ParamAccess.item);
            pManager.AddGenericParameter("Length Count", "LC", "The number of boards in the length", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The bilayer panel object", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.WorldXY;
            DA.GetData(0, ref plane);

            double boardWidth = 0;
            DA.GetData(1, ref boardWidth);

            double boardLength = 0;
            DA.GetData(2, ref boardLength);

            double widthCount = 0;
            DA.GetData(3, ref widthCount);

            double lengthCount = 0;
            DA.GetData(4, ref lengthCount);

            Panel panel = new Panel(plane,boardWidth, boardLength, (int)widthCount, (int)lengthCount);

            DA.SetData(0, panel);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E8C93714-BDAD-459B-A1E1-CA76D33742B5");
    }
}