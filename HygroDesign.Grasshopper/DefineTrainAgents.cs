using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using HygroDesign.Core;
using ABxM.Core;

using Tensorflow;
using Tensorflow.NumPy;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;
using System.IO;
using ABxM.Core.Agent;

using System.Linq;

namespace HygroDesign.Grasshopper.Components
{

    public class TrainCrossSectionAgents : GH_Component
    {

        public TrainCrossSectionAgents()
          : base("Train cross section agents", "Train",
            "Train cross section agents with a given reward.",
            "HygroDesign", "DQL")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Agents", "A", "The cross section agent to train", GH_ParamAccess.list);
            pManager.AddNumberParameter("Reward", "R", "The reward for the current behaviour", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Random random = new Random();
            List<CrossSectionAgent> agents = new List<CrossSectionAgent>();
            List<double> rewards = new List<double>();

            if (!DA.GetDataList(0,agents)) return;
            if (!DA.GetDataList(1, rewards)) return;

            if (agents.Count != rewards.Count) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Agent count and reward count don't match.");

            for(int i = 0; i < agents.Count; i++)
            {
                agents[i].Reward = rewards[i];
            }

            CrossSectionAgentSystem agentSystem = agents[0].AgentSystem as CrossSectionAgentSystem;
            if(agentSystem.Trainer.Models.Count==0)return;

            foreach (CrossSectionAgent agent in agents)
            {
                Tuple<List<double>, int, double, List<double>> memorySample = new Tuple<List<double>, int, double, List<double>>(agent.PrevState, agent.Action, agent.Reward, agent.StateIn);
                
                agent.Epsilon = agentSystem.Trainer.Train(agent.Id-1, memorySample, agent.Epsilon);
                RhinoApp.WriteLine("trained");
                agent.PrevState = agent.StateIn;

                /*
                int i = agent.Behaviors[0].Solver.IterationCount;
                if (i % ModelSaveFrequency == 0 && i > 0)
                {
                    agent.Model.save(Path.Combine(ModelSavePath, String.Format("{0}.h5", i)));
                }
                */
            }
            
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("98498888-E5B8-40C2-B4BE-48670E9D90AD");
    }
}