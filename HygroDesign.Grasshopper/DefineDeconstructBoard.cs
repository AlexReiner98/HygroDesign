using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDeconstructBoard : GH_Component
    {
        
        public DefineDeconstructBoard()
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
            pManager.AddGenericParameter("Polyline", "P", "The board polyline.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Name", "N", "The board name.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "The board material.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Moisture Change", "MC", "The board moisture change.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius", "R", "The pure timoshenko prediction of the board's radius.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Blended Radius", "BR", "The blended radius which results from the curvature convolution.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thickness Blended Radius", "TBR", "The blended radius which results from a weighted average of this board and its thickness neighbors, after the curvature convolution.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PanelBoard board = null;
            DA.GetData(0, ref board);

            DA.SetData("Material", board.Material.Name);
            DA.SetData("Polyline", board.Polyline) ;
            DA.SetData("Radius", board.Radius);
            DA.SetData("Moisture Change", board.MoistureChange);
            DA.SetData("Name", board.Name);
            DA.SetData("Blended Radius", board.BlendedRadius);
            DA.SetData("Thickness Blended Radius", board.ThicknessBlendedRadius);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("61BEB45D-CA43-4BDA-90CA-46358FC192C9");
    }
}