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

    public class DefineDeconstructSpecies : GH_Component
    {
        
        public DefineDeconstructSpecies()
          : base("Deconstruct Species", "Dec Secies",
            "Deconstructs a Species object into its name and properties.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Species", "S", "Species to deconstruct.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Name", "N", "The species name.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Species species = null;
            DA.GetData(0, ref species);

            DA.SetData(0, species.Name);
            //populate outputs based on attributes dictionary
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("51C75C04-3F1C-4DF1-B5B4-84978C567EFA");
    }
}