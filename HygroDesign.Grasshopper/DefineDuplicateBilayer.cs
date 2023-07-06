using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineDuplicateBilayer : GH_Component
    {
        
        public DefineDuplicateBilayer()
          : base("Duplicate Bilayer", "Dup Bilayer",
            "Duplicate Bilayer",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "B", "The bilayer to be duplicated.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "C", "The number of bilayers to create.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayers", "B", "The list of duplicate bilayers", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bilayer bilayer = null;
            DA.GetData(0, ref bilayer);

            int count = 0;
            DA.GetData(1, ref count);

            List<Bilayer> bilayers = new List<Bilayer>();
            for (int i = 0; i < count; i++)
            {
                bilayers.Add(Bilayer.DeepCopy(bilayer));
            }

            DA.SetDataList(0, bilayers);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("9779CD54-BE72-42B8-9F8B-DFD6121946AF");
    }
}