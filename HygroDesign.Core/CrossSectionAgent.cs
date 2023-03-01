using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using ABxM.Core.Agent;
using ABxM.Core.Behavior;

namespace HygroDesign.Core
{
    public class CrossSectionAgent : CartesianAgent
    {
        public CrossSectionAgentSystem CrossSectionAgentSystem;

        public CrossSectionAgent(Point3d startPosition, List<BehaviorBase> behaviours)
            : base(startPosition, behaviours)
        {
            StartPosition = Position = startPosition;
        }

        public override void Reset()
        {
            base.Reset();
            Position = StartPosition;
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