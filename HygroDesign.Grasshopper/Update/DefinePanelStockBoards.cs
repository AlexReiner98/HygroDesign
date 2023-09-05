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

    public class DefinePanelStockBoards : GH_Component
    {

        public DefinePanelStockBoards()
          : base("Panel StockBoards", "Pan Stock",
            "Update a panel's stockboards.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("StockBoards", "S", "The stockboards to add to the panel.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel oldpanel = null;
            DA.GetData(0, ref oldpanel);
            Panel panel = Panel.DeepCopy(oldpanel);

            GH_Structure<IGH_Goo> boards = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(1, out boards);
            if (boards.Branches.Count != panel.Bilayers.Count) throw new Exception("Number of board branches must be equal to number of bilayers in the panel.");

            for(int i = 0; i < boards.Branches.Count; i++)
            {
                if (boards.Branches[i].Count != panel.Bilayers[i].ActiveLayer.Boards.Count) throw new Exception("Number of stock boards within the branch must be equal to the number of active layer boards in the bilayer.") ;
                for(int j = 0; j < boards.Branches[i].Count; j++)
                {
                    boards.Branches[i][j].CastTo(out StockBoard board);
                    panel.Bilayers[i].ActiveLayer.Boards[j].StockBoard = board;
                }
            }

            DA.SetData(0, panel);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("BBE1A691-C6B2-4582-B458-370C4362D4AE");
    }
}