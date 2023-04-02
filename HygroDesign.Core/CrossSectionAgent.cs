using System.Collections.Generic;
using Rhino.Geometry;

using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using HygroDesign.Core.DQL;

namespace HygroDesign.Core
{ 
    public class CrossSectionAgent : CartesianAgent
    {
        public AgentModel model;
        public int Action;
        public double Reward;
        public double[] StateIn;
        public double[] PrevState;
        public double[] ResetState;
        public int Perception;
        public int perceptionStart;
        public int perceptionEnd;

        public CrossSectionAgentSystem CrossSectionAgentSystem;

        public CrossSectionAgent(Point3d startPosition, List<BehaviorBase> behaviors) : base(startPosition,behaviors)
        {
        }

        public override void Reset()
        {
            base.Reset();
            model = null;
            Reward = 0;
            StateIn = ResetState;
            ResetState = null;
            perceptionEnd = -1;
            perceptionStart = -1;
        }

        public void SoftReset()
        {
            base.Reset();
            Reward = 0;
            StateIn = ResetState;
            ResetState = null;
            perceptionEnd = -1;
            perceptionStart = -1;
        }

        public override void PreExecute()
        {
            base.PreExecute();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void PostExecute()
        {
            base.PostExecute();
        }

        public override List<object> GetDisplayGeometries() => new List<object>()
    {
      (object) this.Position
    };

    }
}
