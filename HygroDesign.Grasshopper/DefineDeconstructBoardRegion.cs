using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDeconstructBoardRegion : GH_Component
    {
        
        public DefineDeconstructBoardRegion()
          : base("Deconstruct Board Region", "Dec Reg",
            "Deconstructs a BoardRegion object.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Board Region", "BR", "BoardRegion to deconstruct.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Polyline", "P", "The region polyline.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Name", "N", "The region's parent's board name.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Species", "S", "The region's parent's species.", GH_ParamAccess.item);
            pManager.AddGenericParameter("RT Angle", "RT", "The region's parent's RT angle", GH_ParamAccess.item);
            pManager.AddGenericParameter("Moisture Change", "MC", "The region's parent's moisture change.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Desired Radius", "DR", "The original desired radius of the region's parent.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius", "R", "The pure timoshenko prediction of the region's parent's radius.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Blended Radius", "BR", "The region's parent's blended radius which results from the curvature convolution.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thickness Blended Radius", "TBR", "The parent board's radius blended with the other board regions parent board radii in the board stack.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BoardRegion region = null;
            DA.GetData(0, ref region);

            if(region != null )
            {
                DA.SetData("Polyline", region.Polyline);
                DA.SetData("Name", region.Name);
                DA.SetData("Species", region.Species);
                DA.SetData("RT Angle", region.RTAngle);
                DA.SetData("Moisture Change", region.MoistureChange);
                DA.SetData("Desired Radius", region.DesiredRadius);
                DA.SetData("Radius", region.Radius);
                DA.SetData("Blended Radius", region.BlendedRadius);
                DA.SetData("Thickness Blended Radius", region.ThicknessBlendedRadius);
            }
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("021B4F37-8181-4DAE-A728-7CA536AF1A2A");
    }
}