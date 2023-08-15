using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Grasshopper.Kernel.Parameters;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineStockBoards : GH_Component, IGH_VariableParameterComponent
    {
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index > 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            //We can only remove from the input
            if (side == GH_ParameterSide.Input && index > 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Param_GenericObject param = new Param_GenericObject()
            {
                Name = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input)
            };
            param.NickName = param.Name;
            param.Description = "Things to be sent around.";
            param.Optional = true;
            param.Access = GH_ParamAccess.list;

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
        }

        public DefineStockBoards()
          : base("Stock Boards", "Stock Boards",
            "Define a set of stock which will be used to create the bilayer design.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Name", "N", "Name or identifier sting to keep track of the baords.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Species", "S", "Wood species of the stockpile.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Thickness", "T", "Board thickness.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Length", "L", "The list of lengths for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Width", "W", "The list of widths for the boards.", GH_ParamAccess.list);
            pManager.AddGenericParameter("RT Angle", "RT", "The list of RT angles for the boards.", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Stock Boards", "S", "The list of boards generated.", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {



            List<Species> species = new List<Species>();
            DA.GetDataList("Species", species);

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



            var paramDictionary = new List<Dictionary<string, double>>();
            List<double> ghInputProperty = new List<double>();
            double valueExtract = 0;

            for (int p = 6; p < Params.Input.Count; p++)
            {
                var key = Params.Input[p].NickName;
                DA.GetDataList(p, ghInputProperty);

                for (int i = 0; i < lengths.Count; i++)
                {
                    paramDictionary.Add(new Dictionary<string, double>());
                    valueExtract = ghInputProperty[i];
                    paramDictionary[i].Add(key, valueExtract);
                }
            }


            List<StockBoard> boards = new List<StockBoard>();
            for(int i = 0; i < lengths.Count; i++)
            {
                boards.Add(new StockBoard(names[i], species[i], rts[i], thicknesses[i],lengths[i], widths[i], paramDictionary[i]));
            }

            DA.SetDataList(0, boards);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("DA3AD997-D812-4644-A4AB-F23CBD3A39B4");
    }
}