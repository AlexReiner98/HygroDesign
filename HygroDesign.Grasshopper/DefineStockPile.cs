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

    public class DefineStockpile : GH_Component
    {
        
        public DefineStockpile()
          : base("Stockpile", "Stockpile",
            "Define a set of stock which will be used to create the bilayer design.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Wood species of the stockpile.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Name", "N", "Name or identifier sting to keep track of the baords.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Length", "L", "The list of lengths for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Width", "W", "The list of widths for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("RT Angle", "RT", "The list of RT angles for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Multiplier", "M", "An optional list of factors for timoshenko equation based on error by RT angle.", GH_ParamAccess.list);
            pManager[5].Optional = true;
            pManager.AddGenericParameter("Moisture Changes", "WMCC", "The acceptable moisture change steps during fabrication.", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Stockile", "S", "The stockpile for panel fabrication.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Material material = null;
            DA.GetData(0, ref material);

            List<string> names = new List<string>();
            DA.GetDataList(1, names);

            List<double> lengths = new List<double>();
            DA.GetDataList(2, lengths);

            List<double> widths = new List<double>();
            DA.GetDataList(3, widths);

            List<double> rts = new List<double>();
            DA.GetDataList(4, rts);

            List<double> multipliers = new List<double>();
            DA.GetDataList(5, multipliers);

            List<double> wmccs = new List<double>();
            DA.GetDataList(6, wmccs);

            List<StockBoard> boards = new List<StockBoard>();
            for(int i = 0; i < lengths.Count; i++)
            {
                boards.Add(new StockBoard(names[i], material, rts[i], lengths[i], widths[i]));
            }

            if (multipliers.Count == 0 | multipliers == null) foreach (StockBoard board in boards) multipliers.Add(1.0);

            StockPile stockpile = new StockPile(material, boards, wmccs, multipliers);

            DA.SetData(0, stockpile);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("DA3AD997-D812-4644-A4AB-F23CBD3A39B4");
    }
}