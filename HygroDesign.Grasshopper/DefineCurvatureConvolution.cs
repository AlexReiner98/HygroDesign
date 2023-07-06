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
            pManager.AddNumberParameter("Stiffness Factor", "S", "The factor used to control the weight variations in stiffness are given to the weighted average", GH_ParamAccess.item, 1.0);
            pManager.AddGenericParameter("Visualize Board Weights", "V", "The index values of the board whose neighbours weights should be visualized", GH_ParamAccess.list);
            pManager[5].Optional = true;
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

            double stiffnessFactor = 0;
            DA.GetData(4, ref stiffnessFactor);

            List<int> indexes = new List<int>();
            DA.GetDataList(5, indexes);

            List<List<List<double>>> weightsArray = new List<List<List<double>>>();

            for(int i = 0; i < panels.Count; i++)
            {
                panels[i] = Panel.DeepCopy(panels[i]);
                panels[i].SetConvolutionFactors(maxRad, stiffnessFactor);
                panels[i].CurvatureConvolution(longitudinal, tangential);

                //if (indexes.Count != 2 | indexes.Count != 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "indexes must be two interger values"); return; }
                if(indexes.Count == 2) weightsArray.Add(panels[i].GetNeighborWeights(indexes));

            }
            DA.SetDataList(0, panels);

            if (indexes.Count != 2) return;
            DataTree<double> weights = new DataTree<double>();
            for(int i = 0; i < weightsArray.Count; i++)
            {
                for(int j = 0; j< weightsArray[i].Count; j++)
                {
                    for(int v = 0; v < weightsArray[i][j].Count;v++)
                    {
                        weights.Add(weightsArray[i][j][v], new GH_Path(i,j));
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