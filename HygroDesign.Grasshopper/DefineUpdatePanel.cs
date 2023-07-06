using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineUpdatePanel : GH_Component
    {
        
        public DefineUpdatePanel()
          : base("Update Panel", "Update Panel",
            "Update an existing panel with board goalset.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "Panel to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius", "R", "Desired radius of board.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Radius Weight", "RW", "Importance factor for this board's radius.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Material", "M", "Desired material of board.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Material Weight", "MW", "Importance factor for this board's material.", GH_ParamAccess.tree);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            GH_Structure<IGH_Goo> radius = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(1, out radius);

            GH_Structure<IGH_Goo> radiusWeight = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(2, out radiusWeight);

            GH_Structure<IGH_Goo> material = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(3, out material);

            GH_Structure<IGH_Goo> materialWeight = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(4, out materialWeight);

            Panel panelCopy = Panel.DeepCopy(panel);

            for(int i = 0; i < panelCopy.Boards.Length; i++)
            {
                for(int j = 0; j < panelCopy.Boards[i].Length; j++)
                {
                    radius.Branches[i][j].CastTo<double>(out double DesiredRadius);
                    panelCopy.Boards[i][j].DesiredRadius = DesiredRadius;

                    radiusWeight.Branches[i][j].CastTo<double>(out double RadiusWeight);
                    panelCopy.Boards[i][j].RadiusWeight = RadiusWeight;

                    material.Branches[i][j].CastTo<object>(out object Material);
                    panelCopy.Boards[i][j].DesiredMaterial = Material as Material;

                    materialWeight.Branches[i][j].CastTo<double>(out double MaterialWeight);
                    panelCopy.Boards[i][j].MaterialWeight = MaterialWeight;
                }
            }

            DA.SetData(0, panelCopy);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("CD8B83DA-3C93-4622-8D5A-CC8B74B84029");
    }
}