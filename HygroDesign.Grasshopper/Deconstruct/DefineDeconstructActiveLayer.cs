using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Deconstruct
{

    public class DefineDeconstructActiveLayer : GH_Component
    {

        public DefineDeconstructActiveLayer()
          : base("Deconstruct Active Layer", "Dec Active Layer",
            "Deconstruct an active layer into its component parts.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Active Layer", "AL", "The active layer to deconstruct.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Active Layer Boards", "AB", "The list of boards making up the active layer", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Thickness", "T", "The active layer's thickness.", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTree<ActiveBoard> boardTree = new DataTree<ActiveBoard>();
            DataTree<double> thicknessTree = new DataTree<double>();

            GH_Structure<IGH_Goo> activeLayers = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out activeLayers);
            for (int i = 0; i < activeLayers.Branches.Count; i++)
            {
                for (int j = 0; j < activeLayers.Branches[i].Count; j++)
                {
                    ActiveLayer activeLayer = null;
                    activeLayers.Branches[i][j].CastTo<ActiveLayer>(out activeLayer);

                    GH_Path path = activeLayers.Paths[i].AppendElement(j);
                    boardTree.AddRange(activeLayer.Boards, path);
                    thicknessTree.Add(activeLayer.Thickness, path);
                }
            }

            DA.SetDataTree(0, boardTree);
            DA.SetDataTree(1, thicknessTree);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("6AC98335-4B11-4358-9608-D2E334BE0D51");
    }
}