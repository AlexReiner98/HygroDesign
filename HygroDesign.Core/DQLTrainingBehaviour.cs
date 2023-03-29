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
//using Python.Runtime;
//using Tensorflow;


namespace HygroDesign.Core
{
    public class DQLTrainingBehaviour : BehaviorBase
    {
        private int thisPerception;

       

        public DQLTrainingBehaviour( int perception)
        {
            thisPerception = perception;
        }

        //Update agent position based on chosen behaviour
        public void UpdateAgent(CrossSectionAgent agent)
        {
            if (agent.Action == 0) agent.Moves.Add(Vector3d.ZAxis * 0.1); agent.Weights.Add(1);
            if (agent.Action == 1) agent.Moves.Add(Vector3d.ZAxis * 0); agent.Weights.Add(1);
            if (agent.Action == 2) agent.Moves.Add(Vector3d.ZAxis * -0.1); agent.Weights.Add(1);

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
                CSAgent.InputLength = (CSAgent.perceptionEnd - CSAgent.perceptionStart) + 1;
                RhinoApp.WriteLine(CSAgent.Perception.ToString());
            }

            //build agent dql model
            if (CSAgent.AgentModel == null) 
            {
                CSAgent.AgentModel = new AgentModel(CSAgent.InputLength, CSAgent.OutputLength);
                CSAgent.AgentModel.Memory = new List<Tuple<List<double>, int, double, List<double>>>();
                RhinoApp.WriteLine("built, input = " + CSAgent.InputLength.ToString() + " output = " + CSAgent.OutputLength.ToString()); 
            }

            //create inputs
            CSAgent.Inputs.Clear();
            for (int i = CSAgent.perceptionStart; i <= CSAgent.perceptionEnd; i++)
            {
                CrossSectionAgent currentAgent = csAS.Agents[i] as CrossSectionAgent;
                CSAgent.Inputs.Add(currentAgent.Position[2]);
            }
            

            CSAgent.StateIn = CSAgent.Inputs;
            if (CSAgent.ResetState == null) CSAgent.ResetState = CSAgent.StateIn;
            RhinoApp.WriteLine(CSAgent.StateIn.ToString());
            CSAgent.Action = CSAgent.AgentModel.GetAction(CSAgent.StateIn, CSAgent.Epsilon, CSAgent.OutputLength);
            RhinoApp.WriteLine(CSAgent.Action.ToString());
            UpdateAgent(CSAgent);
        }
    }
}
