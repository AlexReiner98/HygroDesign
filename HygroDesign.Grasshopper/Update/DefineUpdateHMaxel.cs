using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Update
{

    public class DefineUpdateHMaxel : GH_Component
    {

        public DefineUpdateHMaxel()
          : base("Update HMaxel", "Up HMaxel",
            "Update an HMaxel with its height.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxel", "HM", "The HMaxel to update.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Height", "H", "The maximum height allowed in this HMaxel.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Radius Factor", "RF", "A value between 0 and 1 that will be remaped to the possible radius range.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Species", "S", "The set of desired species for each HMaxel.",GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HMaxel", "HM", "The updated HMaxel.", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTree<HMaxel> hmaxelOutput = new DataTree<HMaxel>();

            GH_Structure<IGH_Goo> oldhmaxels = new GH_Structure<IGH_Goo> ();
            DA.GetDataTree(0, out oldhmaxels);
            GH_Structure<IGH_Goo> heights = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(1, out heights);
            GH_Structure<IGH_Goo> radiusFactors = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(2, out radiusFactors);
            GH_Structure<IGH_Goo> species = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(3, out species);


            if (oldhmaxels.Branches.Count != heights.Branches.Count || oldhmaxels.Branches.Count != radiusFactors.Branches.Count || oldhmaxels.Branches.Count != species.Branches.Count)
            {
                throw new Exception("Input trees must have same number of branches and HMaxels must be grafted.");
            }

            RhinoApp.WriteLine(oldhmaxels.Branches.Count.ToString());
            for(int i = 0; i < oldhmaxels.Branches.Count; i++)
            {
                List<Species> speciesList = new List<Species>();
                oldhmaxels.Branches[i][0].CastTo(out HMaxel oldHMaxel);
                heights.Branches[i][0].CastTo(out double height);
                radiusFactors.Branches[i][0].CastTo(out double radiusFactor);
                for(int j = 0; j < species.Branches[i].Count; j++)
                {
                    species.Branches[i][j].CastTo(out Species thisSpecies);
                    speciesList.Add(thisSpecies);
                }
                HMaxel hmaxel = HMaxel.DeepCopy(oldHMaxel, oldHMaxel.Panel);
                hmaxel.Height = height;
                hmaxel.RadiusParameter = radiusFactor;
                hmaxel.Species = speciesList;

                hmaxelOutput.Add(hmaxel, oldhmaxels.Paths[i]);
            }
            DA.SetDataTree(0, hmaxelOutput);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("4827BDFB-38E8-4BE4-8A1A-D9115F4ABF4F");
    }
}