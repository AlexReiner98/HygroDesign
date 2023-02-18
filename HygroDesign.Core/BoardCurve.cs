using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System.Linq;


namespace HygroDesign.Core
{
    public class BoardCurve
    {
        /// <summary>
        /// The parent Nurbs Curve from which this segment was constructed
        /// </summary>
        public NurbsCurve ParentCurve;

        /// <summary>
        /// A single arc curve within a cross section.
        /// </summary>
        public Curve Curve;

        /// <summary>
        /// The mid point on the board curve segment.
        /// </summary>
        public Point3d MidPoint;

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
            ParentCurve = parent;
            Curve = parent.ToNurbsCurve(bounds);
            Curve.Domain = new Interval(0, 1);
            MidPoint = Curve.PointAt(0.5);

            //get closest control point
            ControlPointID = ClosestControlPoint(MidPoint);

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


        public int ClosestControlPoint(Point3d point)
        {
            Line midLine = new Line(ParentCurve.PointAtStart, ParentCurve.PointAtEnd);

            NurbsCurvePointList controlPoints = ParentCurve.Points;

            int closestID = 0;
            double closestDist = double.MaxValue;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (i == 0 | i == controlPoints.Count - 1) continue;
                Point3d controlPoint = midLine.ClosestPoint(controlPoints[i].Location,true);
                point = midLine.ClosestPoint(point,true);
                double distance = point.DistanceTo(controlPoint);

                if (distance < closestDist)
                {
                    closestID = i;
                    closestDist = distance;
                }
            }
            return closestID;
        }
    }
}