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

    public class UpdatePanel : GH_Component
    {
        
        public UpdatePanel()
          : base("Update Panel", "Update Panel",
            "Update an existing panel with board goalset.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "Panel to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Boards", "B", "Boards with goal set.", GH_ParamAccess.tree);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            GH_Structure<IGH_Goo> boards = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(1, out boards);

            for(int i = 0; i < panel.Boards.Length; i++)
            {
                for(int j = 0; j < panel.Boards[i].Length; j++)
                {
                    PanelBoard board = boards.Branches[i][j] as PanelBoard;
                    panel.Boards[i][j] = board;
                }
            }

            DA.SetData(0, panel);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("CD8B83DA-3C93-4622-8D5A-CC8B74B84029");
    }
}