using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineSpecies : GH_Component
    {
        
        public DefineSpecies()
          : base("Wood Species", "Species",
            "Define a wood species with material properties relevant to self-shaping curvature prediction.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Name", "N", "Name of the wood species.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Density", "D", "Density of the species in kg/m3", GH_ParamAccess.item);
            pManager.AddGenericParameter("LExpansion", "LX", "Expansion coefficient in the L grain direction.", GH_ParamAccess.item);
            pManager.AddGenericParameter("RExpansion", "RX", "Expansion coefficient in the R grain direction.", GH_ParamAccess.item);
            pManager.AddGenericParameter("TExpansion", "TX", "Expansion coefficient in the T grain direction.", GH_ParamAccess.item);
            pManager.AddGenericParameter("LElasticModulus", "LE", "Elastic modulus in the L grain direction.", GH_ParamAccess.item);
            pManager.AddGenericParameter("RElasticModulus", "RE", "Elastic modulus in the R grain direction.", GH_ParamAccess.item);
            pManager.AddGenericParameter("TElasticModulus", "TE", "Elastic modulus in the T grain direction.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Species", "S", "The wood species.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = null;
            DA.GetData(0, ref name);

            double Density = 0;
            double LExpansion = 0;
            double RExpansion = 0;
            double TExpansion = 0;
            double LElasticModulus = 0;
            double RElasticModulus = 0;
            double TElasticModulus = 0;

            DA.GetData(1, ref Density);
            DA.GetData(2, ref LExpansion);
            DA.GetData(3, ref RExpansion);
            DA.GetData(4, ref TExpansion);
            DA.GetData(5, ref LElasticModulus);
            DA.GetData(6, ref RElasticModulus);
            DA.GetData(7, ref TElasticModulus);

            DA.SetData(0, new Species(name, Density, LExpansion, RExpansion, TExpansion, LElasticModulus, RElasticModulus, TElasticModulus));
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("D7382F08-A34D-4427-ADB9-DD65064A30C5");
    }
}