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
            pManager.AddGenericParameter("Panel", "P", "Panel to deconstruct.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Brep", "B", "Brep to match.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Regions", "B", "The matched region tree.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Faces", "F", "The face that was matched to.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var panels = new List<Panel>();
            DA.GetDataList(0, panels);
            var breps = new List<Brep>();
            DA.GetDataList(1, breps);

            DataTree<BoardRegion> regions = new DataTree<BoardRegion>();
            DataTree<Panel> panelCopies = new DataTree<Panel>();
            DataTree<BrepFace> faceBreps = new DataTree<BrepFace>();


            for (int i = 0; i < panels.Count; i++)
            {
                if (i >= breps.Count) continue;

                GH_Path path = new GH_Path(i);

                Panel panelCopy = Panel.DeepCopy(panels[i]);
                panelCopies.Add(panelCopy, path);
                var regionTree = panelCopy.MatchBrep(breps[i],1);

                for(int v = 0; v < regionTree.Branches.Count; v++)
                {
                    GH_Path fullPath = path.AppendElement(v);
                    regions.AddRange(regionTree.Branch(v), fullPath);

                    for(int x = 0; x < regionTree.Branch(v).Count; x++)
                    {
                        if (regionTree.Branch(v)[x] == null) continue;
                        faceBreps.Add(regionTree.Branch(v)[x].TrimmedRegion, fullPath);
                    }
                }
                
            }
            DA.SetDataTree(0, panelCopies);
            DA.SetDataTree(1, regions);
            DA.SetDataTree(2, faceBreps);

        }



        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("AAB3460C-CD7D-4988-BBF0-61B2C5A00BBE");
    }
}