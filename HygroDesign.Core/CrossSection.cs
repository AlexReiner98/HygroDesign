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
    public class CrossSection
    {
        /// <summary>
        /// The list of arcs describing the geometry of the cross section.
        /// </summary>
        public List<Arc> Arcs = new List<Arc>();

        /// <summary>
        /// The list of arcs describing the geometry of the cross section.
        /// </summary>
        public List<BoardArc> BoardArcs = new List<BoardArc>();

        /// <summary>
        /// The list of segmented curves accurately describing the original nurbs curve
        /// </summary>
        public List<BoardCurve> BoardCurves = new List<BoardCurve>();

        

        /// <summary>
        /// Create a cross section from a set of curvatures
        /// </summary>
        /// <param name="curvatures">Radii for each board.</param>
        /// <param name="directions">Curvature directions for each board.</param>
        /// <param name="boardWidth">Board width constant.</param>
        /// <param name="snapDistance">Snap ends tolerance for creating closed cross sections.</param>
        /// <param name="supportParameters">Parameters along cross section that will be oriented to the base plane.</param>
        /// <param name="basePlane">Base plane for cross section to be oriented to.</param>
        public CrossSection(List<double> curvatures, List<int> directions, double boardWidth, double snapDistance, List<double> supportParameters, Plane basePlane)
        {
            Arcs = CreateArcs(curvatures, directions, boardWidth, basePlane);
            OrientSupportsToBasePlane(supportParameters,basePlane);
            SnapEnds(snapDistance);
        }

        /// <summary>
        /// Create a cross section from a nurbs curve
        /// </summary>
        /// <param name="nurbsCurve">Input nurbs curve to be replicated in cross section form.</param>
        /// <param name="boardWidth">Board width constant.</param>
        /// <param name="minRadius">Minimum possible radius for cross section, derived from bilayer properties.</param>
        public CrossSection(NurbsCurve nurbsCurve, double boardWidth, double minRadius)
        {
            //first analyze curvature of existing board divisions
            // --- avg radius
            // --- radius variation

            AnalyzeDivisionCurvatures(nurbsCurve, boardWidth, minRadius);





            //BoardArcs = NurbsToBoardArcs(nurbsCurve, boardWidth);
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

            //check snapdistance
            if (endsVector.Length > snapDistance) return;

            //curve division
            int divisionCount = Arcs.Count * 2;
            Point3d[] dividePoints;
            double[] divideParameters = joinedCurve.DivideByCount(divisionCount, true, out dividePoints);
            List<Point3d> points = dividePoints.ToList();
            points.Add(joinedCurve.PointAtEnd);

            //point update based on snap vector
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

        /// <summary>
        /// create a list of BoardArcs from an input nurbs curve and a board width
        /// </summary>
        /// <returns></returns>
        public List<BoardArc> NurbsToBoardArcs(NurbsCurve nurbsCurve, double boardWidth)
        {
            List<BoardArc> boardArcs = new List<BoardArc>();

            //precalculate projection plane for closest control point distance
            if (!nurbsCurve.TryGetPlane(out Plane plane));
            else plane = Plane.WorldZX;
            Vector3d rotationAxis = new Vector3d(nurbsCurve.PointAtEnd - nurbsCurve.PointAtStart);
            plane.Rotate(Math.PI * 0.5, rotationAxis);

            double nurbsLength = nurbsCurve.GetLength();
            nurbsCurve.Domain = new Interval(0, 1);

            int boardCount = (int)Math.Ceiling(nurbsLength / boardWidth) + 1;
            double remainderBoardWidth = ((nurbsLength / boardWidth) - Math.Floor(nurbsLength / boardWidth)) * boardWidth;
            if (remainderBoardWidth ==0) boardCount -= 1;

            double endLength;
            double startLength = 0.01;

            for (int i = 0; i < boardCount; i++)
            {
                endLength = (boardWidth * i) + (remainderBoardWidth / 2);

                if (remainderBoardWidth == 0) endLength = boardWidth * i;
                
                if (i == boardCount - 1) endLength = nurbsLength-0.01;
                double midLength = ((endLength - startLength) / 2) + startLength;

                double tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                nurbsCurve.LengthParameter(startLength, out double startParam,tol);
                nurbsCurve.LengthParameter(midLength, out double midParam, tol);
                nurbsCurve.LengthParameter(endLength, out double endParam, tol);

                startLength = endLength;

                Arc arc = new Arc(nurbsCurve.PointAt(startParam), nurbsCurve.PointAt(midParam), nurbsCurve.PointAt(endParam));
                BoardArc boardArc = new BoardArc(arc){ ControlPointID = ClosestControlPoint(nurbsCurve.PointAt(midParam), nurbsCurve, plane) };
                boardArcs.Add(boardArc);
            }
            return boardArcs;
        }

        public int ClosestControlPoint(Point3d point, NurbsCurve nurbsCurve, Plane plane)
        {
            

            NurbsCurvePointList controlPoints = nurbsCurve.Points;

            int closestID = 0;
            double closestDist = double.MaxValue;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                if(i == 0 | i == controlPoints.Count - 1) continue;
                Point3d controlPoint = plane.ClosestPoint(controlPoints[i].Location);
                point = plane.ClosestPoint(point);
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