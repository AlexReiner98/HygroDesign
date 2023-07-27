using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineMatchTrimmedRegionBrep : GH_Component
    {
        
        public DefineMatchTrimmedRegionBrep()
          : base("Match Trimmed Region Brep", "Match Brep",
            "Re-rganizes.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "Panel to deconstruct.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Brep", "B", "Brep to match.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Regions", "B", "The matched region tree.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> panels = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out panels);

            GH_Structure<IGH_Goo> breps = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(1, out breps);

            DataTree<BoardRegion> regions = new DataTree<BoardRegion>();
            DataTree<Panel> panelCopies = new DataTree<Panel>();


            for (int i = 0; i < panels.Branches.Count; i++)
            {
                for(int j = 0; j < panels.Branches[i].Count; j++)
                {
                    GH_Path path = panels.Paths[i].AppendElement(j);

                    panels.Branches[i][j].CastTo<Panel>(out Panel panel);
                    breps.Branches[i][j].CastTo<Brep>(out Brep brep);

                    Panel panelCopy = Panel.DeepCopy(panel);
                    panelCopies.Add(panelCopy, path);
                    var regionTree = panelCopy.MatchBrep(brep,1);

                    for(int v = 0; v < regionTree.Branches.Count; v++)
                    {
                        GH_Path fullPath = path.AppendElement(v);
                        regions.AddRange(regionTree.Branch(v), fullPath);
                    }
                }
            }
            DA.SetDataTree(0, panelCopies);
            DA.SetDataTree(1, regions);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("AAB3460C-CD7D-4988-BBF0-61B2C5A00BBE");
    }
}