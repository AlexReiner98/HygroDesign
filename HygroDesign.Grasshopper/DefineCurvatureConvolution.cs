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
            pManager.AddGenericParameter("Tengential Blending", "T", "The tangential blending factor.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Longitudinal Blending", "L", "The longitudinal blending factor.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Radius Influence", "R", "The maximum radius in for remapping the radius to the radius factor. Lower numbers bias towards lower  blended radii.", GH_ParamAccess.item, 100000);
            pManager.AddIntegerParameter("Visualize Board Weights", "V", "The index values of the board whose neighbours weights should be visualized", GH_ParamAccess.item, 0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The updated bilayer panel.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Weights", "W", "The neighbor weights of the selected board", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            double tangential = 0;
            DA.GetData(1, ref tangential);

            double longitudinal = 0;
            DA.GetData(2, ref longitudinal);

            double maxRad = 0;
            DA.GetData(3, ref maxRad);

            int index = 0;
            DA.GetData(4, ref index);

            var panelsArray = new double[panels.Count][][];

            for (int i = 0; i < panels.Count; i++)
            {
                panels[i] = Panel.DeepCopy(panels[i]);
                for (int j = 0; j < panels[i].Bilayers.Count; j++)
                {
                    panels[i].Bilayers[j].SetConvolutionFactors(maxRad);
                    panels[i].Bilayers[j].CurvatureConvolution(longitudinal, tangential);
                }
                panels[i].ThicknessConvolution();
            }

            for(int i = 0; i < panels.Count; i++)
            {
                var bilayersArray = new double[panels[i].Bilayers.Count][];
                for (int j = 0; j < panels[i].Bilayers.Count; j++)
                {
                    bilayersArray[j] = panels[i].Bilayers[j].GetNeighborWeights(index).ToArray();
                }
                panelsArray[i] = bilayersArray;
            }

            DA.SetDataList(0, panels);

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
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("5ADFA9A4-8B9B-4A86-ACF4-0CFF48BD3224");
    }
}