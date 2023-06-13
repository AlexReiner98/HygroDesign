using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;


namespace HygroDesign.Grasshopper.Components
{
    /*

    public class DefineUpdateBoard : GH_Component
    {
        public DefineUpdateBoard()
          : base("Set Board Goals", "Board Goals",
            "The the material and curvature goals of a board.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Board", "B", "Board to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius", "R", "Desired radius of board.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius Weight", "RW", "Importance factor for this board's radius.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "Desired material of board.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material Weight", "MW", "Importance factor for this board's material.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Board", "B", "The updated board.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PanelBoard board = null;
            DA.GetData(0, ref board);

            double radius = 0;
            DA.GetData(1, ref radius);

            double radiusWeight = 0;
            DA.GetData(2, ref radiusWeight);

            Material material = null;
            DA.GetData(3, ref material);

            double materialWeight = 0;
            DA.GetData(4, ref materialWeight);


            boardCopy.DesiredRadius = radius;
            boardCopy.RadiusWeight = radiusWeight;
            boardCopy.Material = material;
            boardCopy.MaterialWeight = materialWeight;

            DA.SetData(0, boardCopy);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("AC19D41D-B6C2-4FE3-AF4B-DCE112C37008");
    
    }
    */
}