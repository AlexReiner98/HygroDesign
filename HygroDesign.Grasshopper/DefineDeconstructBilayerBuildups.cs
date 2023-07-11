using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Rhino.DocObjects;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDeconstructBilayerBuildups : GH_Component
    {
        
        public DefineDeconstructBilayerBuildups()
          : base("Deconstruct Panel Buildup", "Dec Panel Buildups",
            "Deconstructs the set of layers for each board segment in the panel.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The panels to deconstruct layers of.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Wood Species", "S", "The set of wood species for each board segment.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Thicknesses", "T", "The set of layer thicknesses for each board segment.", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panelList = new List<Panel>();
            DA.GetDataList(0, panelList);
            for(int i = 0; i < panelList.Count; i++) 
            {
                panelList[i] = Panel.DeepCopy(panelList[i]);
            }

            DataTree<Species> species = new DataTree<Species>();
            DataTree<double> thicknesses = new DataTree<double>();

            foreach(Panel panel in panelList)
            {
                int boardID = 0;
                foreach (PanelBoard board in panel.Bilayers[0].Boards)
                {
                    var path = new GH_Path(panel.ID, boardID);
                    species.Add(panel.Bilayers[0].PassiveSpecies, path);
                    species.Add(board.Species, path);
                    thicknesses.Add(panel.Bilayers[0].PassiveThickness, path);
                    thicknesses.Add(panel.Bilayers[0].ActiveThickness, path);
                    
                    if(board.ThicknessNeighbors != null)
                    {
                        foreach (PanelBoard thicknessNeighbor in board.ThicknessNeighbors)
                        {
                            species.Add(thicknessNeighbor.Parent.PassiveSpecies, path);
                            species.Add(thicknessNeighbor.Species, path);
                            thicknesses.Add(thicknessNeighbor.Parent.PassiveThickness, path);
                            thicknesses.Add(thicknessNeighbor.Parent.ActiveThickness, path);
                        }
                    }
                    boardID++;
                }
            }
            DA.SetDataTree(0, species);
            DA.SetDataTree(1, thicknesses);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("67773B25-C4BC-4673-9F97-F45C58B875A6");
    }
}