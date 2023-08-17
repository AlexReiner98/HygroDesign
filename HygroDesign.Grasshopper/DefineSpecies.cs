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
using System.Linq;

namespace HygroDesign.Grasshopper.Components
{

    public class DefineSpecies : GH_Component, IGH_VariableParameterComponent
    {

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index > 0)
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
            if (side == GH_ParameterSide.Input && index>0)
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
            param.Access = GH_ParamAccess.item;

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
        }

        public DefineSpecies()
          : base("Wood Species", "Species",
            "Define a wood species with material properties relevant to self-shaping curvature prediction.",
            "HygroDesign", "Design")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "name", "Name of the wood species.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Species", "S", "The wood species.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var paramDictionary = new Dictionary<string, double>();
            dynamic ghInputProperty = null;
            double valueExtract = 0;

            for (int p = 1; p < Params.Input.Count; p++)
            {
                var key = Params.Input[p].NickName;

                switch (Params.Input[p].Access)
                {
                    case GH_ParamAccess.item:
                        DA.GetData(p, ref ghInputProperty);
                        valueExtract = (double) ghInputProperty?.Value;
                        break;
                    default:
                        continue;
                }

                paramDictionary.Add(key, valueExtract);
            }

            String name = null;
            DA.GetData(0, ref name);

            Species species = new Species(name, paramDictionary);

            DA.SetData(0, species);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("D7382F08-A34D-4427-ADB9-DD65064A30C5");
    }
}