using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using ABxM.Core.Behavior;
using ABxM.Core.Agent;

using HygroDesign.Core.DQL;


namespace HygroDesign.Core
{
    public class DQLTrainingBehaviour : BehaviorBase
    {
        private static int InputLength = 3;
        private static int OutputLength = 2;



        public List<double> Inputs = new List<double>();

        public DQLTrainingBehaviour()
        {
        }

        //Update agent position based on chosen behaviour
        public void UpdateAgent(CrossSectionAgent agent)
        {
            if (agent.Action == 0) agent.Moves.Add(Vector3d.ZAxis); agent.Weights.Add(0.1);
            if (agent.Action == 1) agent.Moves.Add(Vector3d.ZAxis * -1); agent.Weights.Add(0.1);
        }

        public override void Execute(AgentBase agent)
        {

            CrossSectionAgent CSAgent = agent as CrossSectionAgent;

            CrossSectionAgentSystem agentSystem = agent.AgentSystem as CrossSectionAgentSystem;

            if (agentSystem.Trainer.Models.Count < agentSystem.Agents.Count) 
            { 
                agentSystem.Trainer.BuildModel(InputLength, OutputLength); 
                agentSystem.Trainer.Memory.Add(new List<Tuple<List<double>, int, double, List<double>>>());
                RhinoApp.WriteLine("built"); 
            }

            Inputs.Clear();
            Inputs.Add(CSAgent.Position[0]); Inputs.Add(CSAgent.Position[2]);

            CSAgent.StateIn = Inputs;
            if (agentSystem.Trainer.Models.Count < CSAgent.Id + 1) CSAgent.ResetState = CSAgent.StateIn;

            CSAgent.Action = agentSystem.Trainer.GetAction(CSAgent.StateIn, CSAgent.Id, CSAgent.Epsilon, OutputLength);

            UpdateAgent(CSAgent);
        }
    }
}
