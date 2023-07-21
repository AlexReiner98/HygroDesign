using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineBilayer : GH_Component
    {
        
        public DefineBilayer()
          : base("Bilayer", "Bilayer",
            "Generate Bilayer",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Base Plane", "P", "The base plane for the bilayer to be generated from", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Board Width", "W", "The width of a single board in the bilayer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Board Length", "L", "The length of a single board in the bilayer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Width Count", "WC", "The number of boards in the width", GH_ParamAccess.item);
            pManager.AddGenericParameter("Length Count", "LC", "The number of boards in the length", GH_ParamAccess.item);
            pManager.AddGenericParameter("Active Thickness", "AT", "The thickness of the active layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Passive Thickness", "PT", "The thickness of the passive layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Passive Species", "PS", "The wood species of the passive layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Region Count", "RC", "The number of regions per board.", GH_ParamAccess.item);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "B", "The bilayer object", GH_ParamAccess.item);
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

            double activeThickness = 0;
            DA.GetData(5, ref activeThickness);

            double passiveThickness = 0;
            DA.GetData(6, ref passiveThickness);

            Species passiveSpecies = null;
            DA.GetData(7, ref passiveSpecies);

            int boardRegionCount = 0;
            DA.GetData(8, ref boardRegionCount);


            Bilayer bilayer = new Bilayer(plane,boardWidth, boardLength, (int)widthCount, (int)lengthCount, activeThickness, passiveThickness, passiveSpecies, boardRegionCount);

            DA.SetData(0, bilayer);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E8C93714-BDAD-459B-A1E1-CA76D33742B5");
    }
}