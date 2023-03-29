using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using ABxM.Core.Agent;
using ABxM.Core.Behavior;

using static Tensorflow.Binding;
using Tensorflow;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;
using System.IO;
using ABxM.Core.Agent;
using HygroDesign.Core.DQL;

namespace HygroDesign.Core
{
    public class CrossSectionAgent : CartesianAgent
    {
        public CrossSectionAgentSystem CrossSectionAgentSystem;

        public double Utilization = 0;

        //DQL variables
        public int Action;
        public double Reward;
        public List<double> StateIn = new List<double>();
        public List<double> PrevState = new List<double>();
        public List<double> ResetState = null;

        public static double InitialEpsilon = 0.8;
        public double Epsilon = InitialEpsilon;

        public AgentModel AgentModel = null;

        public int InputLength;
        public int OutputLength = 3;
        public int Perception;
        public int perceptionStart;
        public int perceptionEnd;

        public List<double> Inputs = new List<double>();

        public CrossSectionAgent(Point3d startPosition, List<BehaviorBase> behaviours)
            : base(startPosition, behaviours)
        {
            StartPosition = Position = startPosition;
        }

        public override void Reset()
        {
            base.Reset();
            AgentModel = null;
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