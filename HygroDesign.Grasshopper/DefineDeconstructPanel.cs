using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDeconstructPanel : GH_Component
    {
        
        public DefineDeconstructPanel()
          : base("Deconstruct Panel", "Deconstruct Panel",
            "Deconstructs a bilayer panel.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "Panel to deconstruct.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Boards", "B", "The board objects making up the panel.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            DataTree<PanelBoard> output = new DataTree<PanelBoard>();
            
            foreach(Panel panel in panels)
            {
                int index = 0;
                foreach (PanelBoard[] array in panel.Boards)
                {
                    foreach (PanelBoard board in array)
                    {
                        output.Add(board, new GH_Path(panel.ID,index));
                    }
                    index++;
                }
            }
            
            

            DA.SetDataTree(0, output);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E63E8A9D-0CA2-48E7-861B-B1AD77D4FED0");
    }
}