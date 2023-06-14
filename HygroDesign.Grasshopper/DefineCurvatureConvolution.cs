using BilayerDesign;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineCurvatureConvolution : GH_Component
    {
        
        public DefineCurvatureConvolution()
          : base("Curvature Convolution", "Convolution",
            "Perform a curvature convolution which blends the curvatures of neighbouring boards",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to perform the convolution on.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Tengential Blending", "T", "The tangential blending factor.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Longitudinal Blending", "L", "The longitudinal blending factor.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Max Radius Influence", "R", "The maximum radius in for remapping the radius to the radius factor. Lower numbers bias towards lower  blended radii.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated bilayer panel.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            double tangential = 0;
            DA.GetData(1, ref tangential);

            double longitudinal = 0;
            DA.GetData(2, ref longitudinal);

            double maxRad = 0;
            DA.GetData(3, ref maxRad);

            Panel panelCopy = Panel.DeepCopy(panel);

            panelCopy.SetConvolutionFactors(maxRad);
            panelCopy.CurvatureConvolution(longitudinal,tangential);

            DA.SetData(0, panelCopy);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("5ADFA9A4-8B9B-4A86-ACF4-0CFF48BD3224");
    }
}