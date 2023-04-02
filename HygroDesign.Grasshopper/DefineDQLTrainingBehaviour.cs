using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;


using HygroDesign.Core;



namespace HygroDesign.Grasshopper.Components
{

    public class DQLTraining : GH_Component
    {
        

        public DQLTraining()
          : base("DQLTrainingBehaviour", "DQLTraining",
            "Define a behaviour for Deep Q Learning for CrossSectionAgent",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Perception", "P", "The number of neightbours each agent can percieve in each direction", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Behaviour", "B", "The Deep Q Learning Training Behaviour", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int perception = 0;
            DA.GetData(0, ref perception);
            DA.SetData("Behaviour", new DQLTrainingBehaviour(perception));
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("BDCC42BC-EE8A-4343-BDF0-320F082A0A42");
    }
}
