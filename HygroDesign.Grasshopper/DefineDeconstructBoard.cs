using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.IO;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineDeconstructBoard : GH_Component
    {
        
        public DefineDeconstructBoard()
          : base("Deconstruct Board", "Dec Board",
            "Deconstructs a board object.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Board", "B", "Board to deconstruct.", GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Polyline", "P", "The board polyline.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Name", "N", "The board name.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Species", "S", "The board species.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RT Angle", "RT", "The board's RT angle", GH_ParamAccess.tree) ;
            pManager.AddGenericParameter("Moisture Change", "MC", "The board moisture change.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Desired Radius", "DR", "The original desired radius of the board.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Radius", "R", "The pure timoshenko prediction of the board's radius.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Blended Radius", "BR", "The blended radius which results from the curvature convolution.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Shaped Board", "SB", "The 3d surface representing the board after shaping.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Shaped Centroid", "SC", "The point3d representing the board's centroid after shaping.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Board Regions", "BR", "The BoardRegion objects contained by this board.", GH_ParamAccess.tree);

        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> boardStruct = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out boardStruct);

            
            DataTree<string> names = new DataTree<string>();
            DataTree<Polyline> polylines = new DataTree<Polyline>();
            DataTree<double> desiredRadii = new DataTree<double>();
            DataTree<double> RTAngle = new DataTree<double>();
            DataTree<double> radii = new DataTree<double>();
            DataTree<double> mcs = new DataTree<double>();
            DataTree<double> blendedRadii = new DataTree<double>();
            DataTree<BoardRegion> regions = new DataTree<BoardRegion>();
            DataTree<Species> species = new DataTree<Species>();
            DataTree<Surface> surfaces = new DataTree<Surface>();
            DataTree<Point3d> centroids = new DataTree<Point3d>();

            for (int i = 0; i < boardStruct.Branches.Count; i++)
            {
                for(int j = 0; j < boardStruct.Branches[i].Count; j++)
                {
                    GH_Path path = boardStruct.Paths[i];
                    ActiveBoard board;
                    boardStruct[i][j].CastTo<ActiveBoard>(out board);

                    if (board == null) continue;

                    names.Add(board.Name, path);
                    polylines.Add(board.Polyline, path);
                    desiredRadii.Add(board.DesiredRadius, path);
                    RTAngle.Add(board.RTAngle, path);
                    radii.Add(board.Radius, path);
                    mcs.Add(board.MoistureChange, path);
                    blendedRadii.Add(board.BlendedRadius, path);
                    species.Add(board.Species, path);

                    if(board.Parent.Parent != null && board.Parent.Parent.Surface != null)
                    {
                        surfaces.Add(board.ShapedBoard, path);
                        centroids.Add(board.ShapedCentroid, path);
                    }

                    regions.AddRange(board.Regions, path.AppendElement(j));
                }
            }

            DA.SetDataTree(0, polylines);
            DA.SetDataTree(1, names);
            DA.SetDataTree(2, species);
            DA.SetDataTree(3, RTAngle);
            DA.SetDataTree(4, mcs);
            DA.SetDataTree(5, desiredRadii);
            DA.SetDataTree(6, radii);
            DA.SetDataTree(7, blendedRadii);
            DA.SetDataTree(8, surfaces);
            DA.SetDataTree(9, centroids);
            DA.SetDataTree(10, regions);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("61BEB45D-CA43-4BDA-90CA-46358FC192C9");
    }
}