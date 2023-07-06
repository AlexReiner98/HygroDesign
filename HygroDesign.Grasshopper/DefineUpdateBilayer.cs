using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineUpdateBilayer : GH_Component
    {
        
        public DefineUpdateBilayer()
          : base("Update Bilayer", "Update Bilayer",
            "Update an existing bilayer with desired board properties.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "B", "Bilayer to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Radius", "R", "Desired radius of board.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Radius Weight", "RW", "Importance factor for this board's radius.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Desired material of board.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "B", "The updated bilayer.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bilayer bilayer = null;
            DA.GetData(0, ref bilayer);

            var radius = new List<double>();
            DA.GetDataList(1, radius);

            var radiusWeight = new List<double>();
            DA.GetDataList(2, radiusWeight);

            var material = new List<Material>();
            DA.GetDataList(3, material);

            Bilayer bilayerCopy = Bilayer.DeepCopy(bilayer);

            for(int i = 0; i < bilayerCopy.Boards.Count; i++)
            {
                bilayerCopy.Boards[i].DesiredRadius = radius[i];
                bilayerCopy.Boards[i].RadiusWeight = radiusWeight[i];
                bilayerCopy.Boards[i].DesiredMaterial = material[i];
            }

            DA.SetData(0, bilayerCopy);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("CD8B83DA-3C93-4622-8D5A-CC8B74B84029");
    }
}