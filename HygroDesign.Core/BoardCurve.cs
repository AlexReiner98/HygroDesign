using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using System.Linq;


namespace HygroDesign.Core
{
    public class BoardCurve
    {
        /// <summary>
        /// A single arc curve within a cross section.
        /// </summary>
        public Curve Curve;

        /// <summary>
        /// A the radius of the arc.
        /// </summary>
        public double AverageRadius;

        /// <summary>
        /// The curvature bounds in this segment.
        /// </summary>
        public Interval RadiusBounds;

        /// <summary>
        /// The width of the radius bounds.
        /// </summary>
        public double RadiusVariation;

        /// <summary>
        /// The closest NurbsCurve control point id to this arc.
        /// </summary>
        public int ControlPointID;


        /// <summary>
        /// The input arc to create a BoardArc object.
        /// </summary>
        /// <param name="arc"></param>
        public BoardCurve(NurbsCurve parent, Interval bounds)
        {
            Curve = parent.ToNurbsCurve(bounds);
            Curve.Domain = new Interval(0, 1);

            //analyze curvature
            int samplePoints = 10;
            AnalyzeCurvature(samplePoints);
        }


        private void AnalyzeCurvature(int samplePoints)
        {
            Curve.TryGetPlane(out Plane plane);

            double minRadius = double.MaxValue;
            double maxRadius = double.MinValue;

            double stepSize = 1.0 / (samplePoints - 1);

            for(int i = 0; i < samplePoints; i++)
            {
                Vector3d curvature = Curve.CurvatureAt(i * stepSize);
                double direction = OverUnder(plane, curvature);
                double currentRadius = curvature.Length * direction;
                if (currentRadius < minRadius) minRadius = currentRadius;
                if (currentRadius > maxRadius) maxRadius = currentRadius;
                AverageRadius += currentRadius;
            }
            RadiusBounds = new Interval(minRadius, maxRadius);
            RadiusVariation = maxRadius - minRadius;
        }

        private double OverUnder(Plane plane, Vector3d curvature)
        {
            Vector3d positive = plane.YAxis;
            if (Vector3d.VectorAngle(positive, curvature) <= Math.PI * 0.5) return 1.0;
            else return -1.0;
        }
    }
}