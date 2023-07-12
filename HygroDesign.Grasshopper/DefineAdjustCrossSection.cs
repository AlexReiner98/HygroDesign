using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineAdjustCrossSection : GH_Component
    {
        
        public DefineAdjustCrossSection()
          : base("Adjust Cross Section", "Adjust Cro-Sec",
            "Adjust the cross section of a panel by removing boards from added bilayers.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to adjust cross section of", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thicknesses", "T", "Parameters describing how thick each board-area within the panels should be.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel with adjusted cross section", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            GH_Structure<IGH_Goo> thicknesses = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(1, out thicknesses);

            List<List<double>> thickness = new List<List<double>>();
            for(int i = 0; i < thicknesses.Branches.Count; i++)
            {
                List<double> innerBranch = new List<double>();
                for(int j = 0; j < thicknesses.Branches[i].Count ; j++)
                {
                    double thisThickness = 0;
                    thicknesses.Branches[i][j].CastTo<double>(out thisThickness);
                    innerBranch.Add(thisThickness);
                }
                thickness.Add(innerBranch);
            }
            
            Panel newPanel = Panel.DeepCopy(panel);
            newPanel.ApplyThicknessGradient(thickness);
            
            DA.SetData(0, newPanel);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("FD243E1E-8030-43CF-B5D5-03D925DE3E2A");
    }
}