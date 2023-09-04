using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;
using System.Linq;

namespace HygroDesign.Grasshopper.Update
{

    public class DefineTrimmingBreps : GH_Component
    {

        public DefineTrimmingBreps()
          : base("Trimming Breps", "Trim Breps",
            "Update a panel's bilayer trimming with breps.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Breps", "B", "The brep to update the panel with.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel oldpanel = null;
            GH_Structure<IGH_Goo> breps = new GH_Structure<IGH_Goo>();

            DA.GetData(0, ref  oldpanel);
            DA.GetDataTree(1, out breps);

            Panel panel = Panel.DeepCopy(oldpanel);
            if (breps.Branches.Count != panel.Bilayers.Count) throw new Exception("Breps branch count must be equal to number of bilayers in the panel");

            for(int i = 0; i < panel.Bilayers.Count; i++)
            {
                List<Brep> brepList = new List<Brep>();
                for(int j = 0; j < breps.Branches[i].Count; j++)
                {
                    breps[i][j].CastTo(out Brep brep);
                    brepList.Add(brep);
                }

                panel.Bilayers[i].Trim(brepList);
            }
            //check to see if hmaxels no longer overlap with a board and remove them

            DA.SetData(0, panel);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("C6A7D1AA-40F5-4F28-93BA-07574023D39A");
    }
}