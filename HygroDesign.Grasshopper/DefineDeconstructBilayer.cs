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

    public class DefineDeconstructBilayer : GH_Component
    {
        
        public DefineDeconstructBilayer()
          : base("Deconstruct Bilayer", "Deconstruct Bilayer",
            "Deconstructs a bilayer.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "B", "Bilayer to deconstruct.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Boards", "B", "The board objects making up the bilayer.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var bilayers = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out bilayers);

            DataTree<ActiveBoard> output = new DataTree<ActiveBoard>();
            
            for(int i = 0; i < bilayers.Branches.Count; i++)
            {
                for(int j = 0; j < bilayers.Branches[i].Count; j++)
                {
                    Bilayer bilayer;
                    bilayers.Branches[i][j].CastTo<Bilayer>(out bilayer);
                    output.AddRange(bilayer.Boards, new GH_Path(i,j));
                }
            }

            DA.SetDataTree(0, output);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("D6521780-6817-4665-850B-A59046C07312");
    }
}