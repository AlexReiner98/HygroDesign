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

    public class DefineStockBoards : GH_Component
    {
        
        public DefineStockBoards()
          : base("Stock Boards", "Stock Boards",
            "Define a set of stock which will be used to create the bilayer design.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Name", "N", "Name or identifier sting to keep track of the baords.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Species", "S", "Wood species of the stockpile.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thickness", "T", "Board thickness.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Length", "L", "The list of lengths for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Width", "W", "The list of widths for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("RT Angle", "RT", "The list of RT angles for the boards.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "M", "An optional list of factors for timoshenko equation based on error by RT angle.", GH_ParamAccess.list);
            pManager[6].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Stock Boards", "S", "The list of boards generated.", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Species species = null;
            DA.GetData("Species", ref species);

            List<string> names = new List<string>();
            DA.GetDataList("Name", names);

            List<double> thicknesses = new List<double>();
            DA.GetDataList("Thickness", thicknesses);

            List<double> lengths = new List<double>();
            DA.GetDataList("Length", lengths);

            List<double> widths = new List<double>();
            DA.GetDataList("Width", widths);

            List<double> rts = new List<double>();
            DA.GetDataList("RT Angle", rts);

            List<double> multipliers = new List<double>();
            DA.GetDataList("Multiplier", multipliers);

            if(multipliers.Count < rts.Count)
            {
                for(int i= 0; i < rts.Count; i++)
                {
                    multipliers.Add(1.0);
                }
            }

            List<StockBoard> boards = new List<StockBoard>();
            for(int i = 0; i < lengths.Count; i++)
            {
                boards.Add(new StockBoard(names[i], species, rts[i], thicknesses[i],lengths[i], widths[i], multipliers[i]));
            }

            DA.SetDataList(0, boards);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("DA3AD997-D812-4644-A4AB-F23CBD3A39B4");
    }
}