using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;


namespace HygroDesign.Grasshopper.Components
{

    public class DeconstructBoard : GH_Component
    {
        
        public DeconstructBoard()
          : base("Deconstruct Board", "Dec Board",
            "Deconstructs a board object.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Board", "B", "Board to deconstruct.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Board Polyline", "P", "The board polyline.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Board Centroid", "C", "The board centroid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "The board material.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius", "R", "The board radius.", GH_ParamAccess.item);
            pManager.AddGenericParameter("MoistureChange", "MC", "The board moisture change.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Name", "N", "The board name.", GH_ParamAccess.item);

        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PanelBoard board = null;
            DA.GetData(0, ref board);

            Polyline poly = board.Polyline;
            Point3d centroid = board.Centroid;
            string material = board.Material.Name;
            double radius = board.Radius;
            double moistureChange = board.MoistureChange;
            string name = board.Name;
            

            DA.SetData(0, poly);
            DA.SetData(1, centroid);
            DA.SetData(2, material);
            DA.SetData(3, radius);
            DA.SetData(4, moistureChange);
            DA.SetData(5, name);

        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("61BEB45D-CA43-4BDA-90CA-46358FC192C9");
    }
}