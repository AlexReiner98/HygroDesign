using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDuplicatePanel : GH_Component
    {
        
        public DefineDuplicatePanel()
          : base("Duplicate Panel", "Dup Panel",
            "Duplicate Bilayer Panel",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to be duplicated.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "C", "The number of panels to create.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The list of duplicate panels", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            int count = 0;
            DA.GetData(1, ref count);

            List<Panel> panels = new List<Panel>();
            for (int i = 0; i < count; i++)
            {
                panels.Add(Panel.DeepCopy(panel));
            }

            DA.SetDataList(0, panels);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("9AA16D81-A765-4B62-A147-042283AC1002");
    }
}