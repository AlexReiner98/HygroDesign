using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using System.Linq;


namespace HygroDesign.Core
{
    public class CrossSection
    {
        /// <summary>
        /// The list of arcs describing the geometry of the cross section.
        /// </summary>
        public List<Arc> Arcs = new List<Arc>();

        public CrossSection(List<double> curvatures, List<int> directions, double boardWidth, double snapDistance, List<double> supportParameters, Plane basePlane)
        {
            Arcs = CreateArcs(curvatures, directions, boardWidth, basePlane);
            OrientSupportsToBasePlane(supportParameters,basePlane);
            SnapEnds(snapDistance);
        }

        private List<Arc> CreateArcs(List<double> radii, List<int> directions, double boardWidth, Plane basePlane)
        {
            List<Arc> arcs = new List<Arc>();

            Point3d startPoint = basePlane.Origin;
            Vector3d startVector = basePlane.XAxis;

            for (int i = 0; i < radii.Count; i++)
            {
                //create centroid point3d
                Vector3d startToCentroid = new Vector3d(startVector);
                startToCentroid.Rotate(Math.PI * 0.5, basePlane.ZAxis); startToCentroid.Unitize(); startToCentroid *= radii[i];
                if (directions[i] == 1) startToCentroid.Reverse();

                Point3d centroid = startPoint + startToCentroid;

                //calculate angleRadians
                double angleRadians = boardWidth / radii[i];
                if (directions[i] == 1) angleRadians *= -1;

                //calculate end point
                Point3d endPoint = new Point3d(startPoint);
                Transform rotation = Transform.Rotation(angleRadians, basePlane.ZAxis, centroid);
                endPoint.Transform(rotation);

                //create arc
                Arc arc = new Arc(startPoint, startVector, endPoint);
                arcs.Add(arc);

                //update new inputs
                startPoint = endPoint;
                startVector = arc.TangentAt(boardWidth / radii[i]);
            }
            return arcs;
        }

        private void OrientSupportsToBasePlane(List<double> supports, Plane basePlane)
        {
            //join arcs into single nurbs curve for support parameter evaluation
            Curve[] arcsToJoin = new Curve[Arcs.Count];
            for (int i = 0; i < Arcs.Count; i++) arcsToJoin[i] = Arcs[i].ToNurbsCurve();
            Curve[] joinedArcs = Curve.JoinCurves(arcsToJoin, 1);

            //calculate rotation from endPoint-startPoint to origin.XAxis
            double rotationAngle = Vector3d.VectorAngle(new Vector3d(joinedArcs[0].PointAtEnd - joinedArcs[0].PointAtStart), basePlane.XAxis, basePlane);
            Transform rotation = Transform.Rotation(rotationAngle, basePlane.ZAxis, joinedArcs[0].PointAtStart);

            //apply transform
            List<Arc> orientedArcs = new List<Arc>();
            for (int i = 0; i < Arcs.Count; i++)
            {
                Arc arc = Arcs[i];
                arc.Transform(rotation);
                orientedArcs.Add(arc);
            }
            Arcs = orientedArcs;
            return;
        }

        private void SnapEnds(double snapDistance)
        {
            List<Arc> output = new List<Arc>();

            //join arcs into nurbs curve
            Curve[] arcsToJoin = new Curve[Arcs.Count];
            for (int i = 0; i < Arcs.Count; i++) arcsToJoin[i] = Arcs[i].ToNurbsCurve();
            Curve[] joinedArcs = Curve.JoinCurves(arcsToJoin, 1);
            Curve joinedCurve = joinedArcs[0];
            joinedCurve.Domain = new Interval(0, 1);


            //generate ends vector
            Vector3d endsVector = new Vector3d(joinedCurve.PointAtStart - joinedCurve.PointAtEnd);
            RhinoApp.WriteLine(endsVector.Length.ToString() + " " + snapDistance.ToString());
            //check snapdistance
            if (endsVector.Length > snapDistance) return;


            //curve division
            int divisionCount = Arcs.Count * 2;
            Point3d[] dividePoints;
            double[] divideParameters = joinedCurve.DivideByCount(divisionCount, true, out dividePoints);
            List<Point3d> points = dividePoints.ToList();
            points.Add(joinedCurve.PointAtEnd);

            //point update based on snap vector
            //double stepSize = endsVector.Length / points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += (endsVector * i) / (points.Count - 2);
            }

            //create arcs through adjusted points
            for (int i = 0; i < divisionCount - 1; i += 2)
            {
                Arc currentArc = new Arc(points[i], points[i + 1], points[i + 2]);
                output.Add(currentArc);
            }

            //output on success
            Arcs = output;
            return;
        }

    }
}