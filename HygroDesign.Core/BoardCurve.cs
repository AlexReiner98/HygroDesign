using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;


namespace HygroDesign.Core
{
    public class BoardCurve
    {
        /// <summary>
        /// The cross section this segment belongs to
        /// </summary>
        public CrossSection CrossSection;

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
        public BoardCurve(CrossSection parent, Interval bounds)
        {
            CrossSection = parent;
            Curve = CrossSection.NurbsCurve.ToNurbsCurve(bounds);
            Curve.Domain = new Interval(0, 1);
            MidPoint = Curve.PointAt(0.5);

            //CrossSection.NurbsCurve.ClosestPoint(MidPoint,out MidPointParentParameter);

            //set home control point
            MostInfluentialControlPoint();

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
                double currentRadius =  (1/ curvature.Length) * direction;
                if (currentRadius < minRadius) minRadius = currentRadius;
                if (currentRadius > maxRadius) maxRadius = currentRadius;
                AverageRadius += Math.Abs(currentRadius);
            }
            RadiusBounds = new Interval(minRadius, maxRadius);
            RadiusVariation = maxRadius - minRadius;
            AverageRadius /= samplePoints;
        }

        private double OverUnder(Plane plane, Vector3d curvature)
        {
            Vector3d positive = plane.YAxis;
            if (Vector3d.VectorAngle(positive, curvature) <= Math.PI * 0.5) return 1.0;
            else return -1.0;
        }

        private void MostInfluentialControlPoint()
        {
            Point3d basePoint = MidPoint;

            

            Vector3d testVector = CrossSection.CurvePlane.ZAxis;
            testVector.Unitize(); testVector *= 0.01;

            double largestDistance = 0;
            int mostInfluentialPointID = 0;
            for(int i = 0; i < CrossSection.NurbsCurve.Points.Count; i++)
            {
                NurbsCurve testCurve = (NurbsCurve)CrossSection.NurbsCurve.Duplicate();
                int currentCP = i;
                if (i == 0) currentCP = 1;
                if (i == testCurve.Points.Count - 1) currentCP = testCurve.Points.Count - 2;
                testCurve.Points.SetPoint(currentCP, testCurve.Points[currentCP].Location + testVector);
                testCurve.ClosestPoint(basePoint, out double t);
                double distance = testCurve.PointAt(t).DistanceTo(basePoint);
                if( distance > largestDistance) { largestDistance = distance;  mostInfluentialPointID = currentCP; }
            }
            
            ControlPointID = mostInfluentialPointID;
        }



        /*
        public int ClosestControlPoint(Point3d point)
        {
            //generate line between end points - represents a neutral axis
            Line midLine = new Line(ParentCurve.PointAtStart, ParentCurve.PointAtEnd);

            NurbsCurvePointList controlPoints = ParentCurve.Points;

            int closestID = 0;
            int nextClosestID = 0;
            double closestDist = double.MaxValue;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (i == 0 || i == controlPoints.Count - 1) continue;
                Point3d controlPoint = midLine.ClosestPoint(controlPoints[i].Location,true);
                point = midLine.ClosestPoint(point,true);
                double distance = point.DistanceTo(controlPoint);

                if (distance < closestDist)
                {
                    nextClosestID = closestID;
                    closestID = i;
                    closestDist = distance;
                }
            }

            int mostInfluential = 0;
            double farthestDist = 0.0;
            Line baseLine = new Line(ParentCurve.PointAtStart, ParentCurve.PointAtEnd);

            for(int i = 0; i < closestIDs.Length; i++)
            {
                double currentDistance = baseLine.ClosestPoint(controlPoints[closestIDs[i]].Location, true).DistanceTo(controlPoints[closestIDs[i]].Location);
                if (currentDistance > farthestDist) { closestID = closestIDs[i]; farthestDist = currentDistance; }
            }
            return closestID;
        }
        */
    }
}