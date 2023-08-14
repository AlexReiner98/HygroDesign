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
            if (side == GH_ParameterSide.Input)
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
            if (side == GH_ParameterSide.Input && Params.Input.Count > 1)
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
            param.Access = GH_ParamAccess.tree;

            //param.AttributesChanged += (sender, e) => Debug.WriteLine("Attributes have changed! (of param)");
            //param.ObjectChanged += (sender, e) => UpdateMetadata();

            //UpdateMetadata();
            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            //UpdateMetadata();
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
            //pManager.AddTextParameter("Name", "N", "Name of the wood species.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Density", "D", "Density of the species in kg/m3", GH_ParamAccess.item);
            //pManager.AddGenericParameter("LExpansion", "LX", "Expansion coefficient in the L grain direction.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("RExpansion", "RX", "Expansion coefficient in the R grain direction.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("TExpansion", "TX", "Expansion coefficient in the T grain direction.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("LElasticModulus", "LE", "Elastic modulus in the L grain direction.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("RElasticModulus", "RE", "Elastic modulus in the R grain direction.", GH_ParamAccess.item);
            //AddGenericParameter("TElasticModulus", "TE", "Elastic modulus in the T grain direction.", GH_ParamAccess.item);


            
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Species", "S", "The wood species.", GH_ParamAccess.item);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
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
            */

            var paramDictionary = new Dictionary<string, object>();
            dynamic ghInputProperty = null;
            object valueExtract = null;

            for (int p = 0; p < Params.Input.Count; p++)
            {
                var key = Params.Input[p].NickName;

                switch (Params.Input[p].Access)
                {
                    case GH_ParamAccess.item:
                        DA.GetData(p, ref ghInputProperty);
                        valueExtract = ghInputProperty?.Value;
                        break;
                    case GH_ParamAccess.list:
                        var dataValues = new List<dynamic>();
                        DA.GetDataList(p, dataValues);
                        valueExtract = dataValues.Select(x => x.Value).ToList();
                        break;
                    case GH_ParamAccess.tree:
                        var tree = new GH_Structure<IGH_Goo>();
                        DA.GetDataTree(p, out tree);
                        var dict = new Dictionary<string, IEnumerable<object>>();

                        for (int j = 0; j < tree.PathCount; j++)
                        {
                            var branch = tree.Branches[j];
                            var list = new List<object>();
                            for (int k = 0; k < branch.Count; k++)
                            {
                                list.Add(branch[k].GetType().GetProperty("Value").GetValue(branch[k], null));
                            }
                            dict.Add(j.ToString(), list);
                        }

                        paramDictionary.Add(key, dict);
                        continue;
                    default:
                        continue;
                }

                paramDictionary.Add(key, valueExtract);
            }

            DA.SetData(0, paramDictionary);
        }


        
        public override GH_Exposure Exposure => GH_Exposure.primary;

        
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("D7382F08-A34D-4427-ADB9-DD65064A30C5");
    }
}