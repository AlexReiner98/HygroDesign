using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Construct
{

    public class DefineBilayer : GH_Component
    {

        public DefineBilayer()
          : base("Construct Bilayer", "Bilayer",
            "Create a new self shaping bilayer.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel this bilayer will be part of.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Board Length", "BL", "The number of HMaxels long the boards will be.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Row Offsets", "RO", "A list describing the number of HMaxels each row will be offset by.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Passive Species", "PS", "The species of the passive layer", GH_ParamAccess.item);
            pManager.AddNumberParameter("Active Thickness", "AT", "The thickness of the active layer", GH_ParamAccess.item);
            pManager.AddNumberParameter("Passive Thickness", "PT", "The thickness of the passive layer", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bilayer", "P", "The panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel panel = null;
            DA.GetData(0, ref panel);

            int boardLength = 0;
            DA.GetData(1, ref boardLength);

            List<int> rowOffsets = new List<int>();
            DA.GetDataList(2, rowOffsets);

            Species passiveSpecies = null;
            DA.GetData(3, ref passiveSpecies);

            double activeThickness = 0;
            DA.GetData(4, ref activeThickness);

            double passiveThickness = 0;
            DA.GetData(5, ref passiveThickness);

            Bilayer bilayer = new Bilayer(panel, boardLength, rowOffsets, passiveSpecies, activeThickness, passiveThickness);

            DA.SetData(0, bilayer);
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("CA6AD018-A274-4791-B8BB-3B1B8EFD8AB1");
    }
}