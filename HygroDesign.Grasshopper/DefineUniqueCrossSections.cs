using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineUniqueCrossSections : GH_Component
    {
        
        public DefineUniqueCrossSections()
          : base("Unique Cross Sections", "CroSecSet",
            "Get unique cross section board lists.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to analyze.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Boards", "B", "The cross section board lists.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            DataTree<BoardRegion> tree = new DataTree<BoardRegion>();

            foreach (Panel panel in panels)
            {
                List<List<BoardRegion>> boards = panel.GetXRangeSets();

                for (int i = 0; i < boards.Count; i++)
                {
                    for (int j = 0; j < boards[i].Count; j++)
                    {
                        tree.Add(boards[i][j], new GH_Path(panel.ID,i));
                    }
                }
            }
            DA.SetDataTree(0, tree);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("3F884312-F84C-42FC-BDE0-ACE2CFA6C3DF");
    }
}