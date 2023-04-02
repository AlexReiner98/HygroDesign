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
        private int thisPerception;
        private int inputLength;
        private int outputLength = 2;

        public DQLTrainingBehaviour( int perception)
        {
            thisPerception = perception;
        }

        //Update agent position based on chosen behaviour
        public void UpdateAgent(CrossSectionAgent agent)
        {
            if (agent.Action == 0) agent.Moves.Add(Vector3d.ZAxis * 0.1); agent.Weights.Add(1);
            if (agent.Action == 1) agent.Moves.Add(Vector3d.ZAxis * -0.1); agent.Weights.Add(1);
        }

        public override void Execute(AgentBase agent)
        {

            CrossSectionAgent CSAgent = agent as CrossSectionAgent;
            CrossSectionAgentSystem csAS = CSAgent.AgentSystem as CrossSectionAgentSystem;

            CSAgent.Perception = thisPerception;

            //find input length
            if(CSAgent.perceptionStart == -1)
            {
                CSAgent.perceptionStart = CSAgent.Id - thisPerception;
                if (CSAgent.perceptionStart < 0) CSAgent.perceptionStart = 0;
                CSAgent.perceptionEnd = CSAgent.Id + thisPerception;
                if (CSAgent.perceptionEnd > csAS.Agents.Count - 1) CSAgent.perceptionEnd = csAS.Agents.Count - 1;
                inputLength = (CSAgent.perceptionEnd - CSAgent.perceptionStart) + 1;
            }

            //build agent dql model
            if (CSAgent.model == null) 
            {
                CSAgent.model = new AgentModel(inputLength, outputLength);
                RhinoApp.WriteLine("Model built, input length = " + inputLength.ToString() + " output length = " + outputLength.ToString()); 
            }

            //create inputs
            List<double> inputs = new List<double>();
            for (int i = CSAgent.perceptionStart; i <= CSAgent.perceptionEnd; i++)
            {
                CrossSectionAgent currentAgent = csAS.Agents[i] as CrossSectionAgent;
                inputs.Add(currentAgent.Position[2]);
            }
            
            CSAgent.StateIn = inputs.ToArray();
            if (CSAgent.ResetState == null) CSAgent.ResetState = CSAgent.StateIn;
            CSAgent.Action = CSAgent.model.GetAction(CSAgent.StateIn);
            UpdateAgent(CSAgent);
        }
    }
}
