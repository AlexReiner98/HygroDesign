using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
            pManager.AddGenericParameter("Panels", "P", "The panels to perform the convolution on.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Convolution Engine", "E", "The engine defining the relationship between neighbouring boards' radii", GH_ParamAccess.item);

            /*
            pManager.AddGenericParameter("Tengential Blending", "T", "The tangential blending factor.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Longitudinal Blending", "L", "The longitudinal blending factor.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Radius Influence", "R", "The maximum radius in for remapping the radius to the radius factor. Lower numbers bias towards lower  blended radii.", GH_ParamAccess.item, 100000);
            pManager.AddNumberParameter("Stiffness Factor", "SF", "The factor for controlling the effect of stiffness differences on the radius blending.", GH_ParamAccess.item,0.1);
            pManager.AddNumberParameter("Thickness Factor", "TF", "The factor for controlling the effect of thickness differences on the radius blending", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("Visualize Board Weights", "V", "The index values of the board whose neighbours weights should be visualized", GH_ParamAccess.item, 0);
            */
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The updated bilayer panel.", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Weights", "W", "The neighbor weights of the selected board", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            ConvolutionEngine convolutionEngine = null;
            DA.GetData(1, ref convolutionEngine);

            for(int i = 0; i < panels.Count; i++)
            {
                panels[i] = Panel.DeepCopy(panels[i]);
                panels[i] = convolutionEngine.Convolution(panels[i]);
            }

            DA.SetDataList(0, panels);

            /*
            double tangential = 0;
            DA.GetData(1, ref tangential);

            double longitudinal = 0;
            DA.GetData(2, ref longitudinal);

            double maxRad = 0;
            DA.GetData(3, ref maxRad);

            double stiffness = 0;
            DA.GetData(4, ref stiffness);

            double thickness = 0;
            DA.GetData(5, ref thickness);

            int index = 0;
            DA.GetData(6, ref index);
            

            List<Panel> panelCopies = new List<Panel>();
            foreach(Panel panel in panels)
            {
                panelCopies.Add(Panel.DeepCopy(panel));
            }

            var panelsArray = new double[panelCopies.Count][][];

            for (int i = 0; i < panelCopies.Count; i++)
            {
                panelCopies[i] = Panel.DeepCopy(panelCopies[i]);
                
                panelCopies[i].SetConvolutionFactors(maxRad, stiffness);
                panelCopies[i].CurvatureConvolution(longitudinal, tangential);
                panelCopies[i].ThicknessConvolution(thickness);
            }

            for(int i = 0; i < panelCopies.Count; i++)
            {
                var bilayersArray = new double[panelCopies[i].Bilayers.Count][];
                for (int j = 0; j < panelCopies[i].Bilayers.Count; j++)
                {
                    bilayersArray[j] = panelCopies[i].Bilayers[j].GetNeighborWeights(index).ToArray();
                }
                panelsArray[i] = bilayersArray;
            }

            DA.SetDataList(0, panelCopies);

            DataTree<double> weights = new DataTree<double>();
            for(int i = 0; i < panelsArray.Length; i++)
            {
                for(int j = 0; j< panelsArray[i].Length; j++)
                {
                    for(int v = 0; v < panelsArray[i][j].Length;v++)
                    {
                        weights.Add(panelsArray[i][j][v], new GH_Path(i,j));
                    }
                }
            }

            DA.SetDataTree(1, weights);
            */
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("5ADFA9A4-8B9B-4A86-ACF4-0CFF48BD3224");
    }
}