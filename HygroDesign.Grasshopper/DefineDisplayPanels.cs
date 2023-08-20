using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types.Transforms;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDisplayPanels : GH_Component
    {
        
        public DefineDisplayPanels()
          : base("Display Panels", "Display Panel",
            "Display Panel",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The list of panels to display.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Active Boards", "A", "The 3d active layer boards.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Passive Boards", "P", "The 3d passive layer boards.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            List<Panel> newPanels = new List<Panel>();
            foreach(Panel panel in panels)
            {
                newPanels.Add(Panel.DeepCopy(panel));
                
            }

            DataTree<Brep> passive = new DataTree<Brep>();
            DataTree<Brep> active = new DataTree<Brep>();

            
            foreach(Panel panel in newPanels)
            {
                double bilayerStartZ = 0;
                foreach (Bilayer bilayer in panel.Bilayers)
                {
                    //create passive layer
                    
                    Brep passiveLayer = new Box(bilayer.BasePlane,bilayer.PassiveLayer.XDomain,bilayer.PassiveLayer.YDomain, new Interval(bilayerStartZ, bilayerStartZ + bilayer.PassiveLayer.Height)).ToBrep();

                    passive.Add(passiveLayer, new GH_Path(panel.ID, bilayer.ID));

                    //create active layer
                    bilayerStartZ += bilayer.PassiveLayer.Height;

                    int boardID = 0;
                    foreach(ActiveBoard board in bilayer.Boards)
                    {
                        foreach(BoardRegion region in board.Regions)
                        {
                            Brep boardBrep = new Box(bilayer.BasePlane, region.RowRange, region.ColumnRange, new Interval(bilayerStartZ, bilayerStartZ + bilayer.ActiveThickness)).ToBrep();

                            active.Add(boardBrep, new GH_Path(panel.ID, bilayer.ID, boardID));
                        }    
                        boardID++;
                        
                    }
                    bilayerStartZ += bilayer.ActiveThickness;
                }
            }
            DA.SetDataTree(0, active);
            DA.SetDataTree(1, passive);

        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("A9596520-96F9-4C3C-82FE-AB11396F0D18");
    }
}