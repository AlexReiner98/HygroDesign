using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;


namespace HygroDesign.Grasshopper.Components
{

    public class DefineWoodSpecies : GH_Component
    {
        
        public DefineWoodSpecies()
          : base("Wood Species", "Species",
            "The wood species with unique mechanical properties.",  
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Species", "S", "The wood species.", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Material> list = new List<Material>
            {
                DesignEnvironment.BeechMaterial,
                DesignEnvironment.SpruceMaterial
            };

            DA.SetDataList(0, list);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("2DAA1A43-555A-4FC2-8FD0-52C46AF3D4BE");
    }
}