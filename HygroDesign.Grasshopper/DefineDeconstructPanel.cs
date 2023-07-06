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
            "Deconstructs a panel into bilayers.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "Panel to deconstruct.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayers", "B", "The bilayer objects making up the panel.", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> panels = new List<Panel>();
            DA.GetDataList(0, panels);

            DataTree<Bilayer> output = new DataTree<Bilayer>();
            
            for(int i = 0; i < panels.Count; i++)
            { 
                foreach (Bilayer bilayer in panels[i].Bilayers)
                {
                    output.Add(bilayer, new GH_Path(i));
                }  
            }
            
            

            DA.SetDataTree(0, output);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("E63E8A9D-0CA2-48E7-861B-B1AD77D4FED0");
    }
}