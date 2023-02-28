﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;


namespace HygroDesign.Core
{
    public class CrossSection
    {
        /// <summary>
        /// Underlying nurbs curve from which the cross section was built
        /// </summary>
        public NurbsCurve NurbsCurve;

        /// <summary>
        /// Original nurbs curve which can be returned to on reset
        /// </summary>
        public NurbsCurve StartingNurbsCurve;

        /// <summary>
        /// the plane of the nurbscurve
        /// </summary>
        public Plane CurvePlane;

        /// <summary>
        /// the constant board width in this cross section
        /// </summary>
        public double BoardWidth;

        /// <summary>
        /// The list of segmented curves accurately describing the original nurbs curve
        /// </summary>
        public List<BoardCurve> BoardCurves = new List<BoardCurve>();

        /// <summary>
        /// The minimum average radius alowed in this cross section
        /// </summary>
        public double MinimumRadius;

        /// <summary>
        /// Create a deep copy of the cross section
        /// </summary>
        public CrossSection(CrossSection crossSection)
        {
            NurbsCurve = StartingNurbsCurve = new NurbsCurve(crossSection.NurbsCurve);
            BoardWidth = crossSection.BoardWidth;
            CurvePlane = new Plane(crossSection.CurvePlane);
            BoardCurves = NurbsToBoardCurves();
        }

        
        /// <summary>
        /// Create a cross section from a nurbs curve
        /// </summary>
        /// <param name="nurbsCurve">Input nurbs curve to be replicated in cross section form.</param>
        /// <param name="boardWidth">Board width constant.</param>
        public CrossSection(NurbsCurve nurbsCurve, double boardWidth)
        {
            NurbsCurve = StartingNurbsCurve = nurbsCurve;
            BoardWidth = boardWidth;
            NurbsCurve.TryGetPlane(out CurvePlane);

            BoardCurves = NurbsToBoardCurves();
        }

        public List<BoardCurve> NurbsToBoardCurves()
        {
            List<BoardCurve> boardCurves = new List<BoardCurve>();

            double nurbsLength = NurbsCurve.GetLength();

            int boardCount = (int)Math.Ceiling(nurbsLength / BoardWidth) + 1;
            double remainderBoardWidth = ((nurbsLength / BoardWidth) - Math.Floor(nurbsLength / BoardWidth)) * BoardWidth;
            if (remainderBoardWidth == 0) boardCount -= 1;

            double endLength;
            double startLength = 0;

            for (int i = 0; i < boardCount; i++)
            {
                endLength = (BoardWidth * i) + (remainderBoardWidth / 2);
                if (i == boardCount - 1) endLength = nurbsLength;

                NurbsCurve.LengthParameter(startLength, out double startParam);
                NurbsCurve.LengthParameter(endLength, out double endParam);

                BoardCurve boardCurve = new BoardCurve(this, new Interval(startParam, endParam)) ;
                boardCurves.Add(boardCurve);

                startLength = endLength;
            }
            return boardCurves;
        }
        public void SatisfyMinimumRadius(double minRadius)
        {
            MinimumRadius = minRadius;
            double minimumStep = 0.01;
            int safety = 100;

            while(safety >= 0)
            {
                safety--;

                //loop to check clusters
                bool[] memberSmaller = new bool[NurbsCurve.Points.Count];
                int falseCount = 0;
                for (int i = 0; i < BoardCurves.Count; i++)
                {
                    if (BoardCurves[i].AverageRadius < MinimumRadius) memberSmaller[BoardCurves[i].ControlPointID] = true;
                    else falseCount++;
                }
                if (falseCount == BoardCurves.Count) return;

                List<Tuple<int, Point3d>> nurbsPoints = new List<Tuple<int, Point3d>>();
                //loop to update control points
                for (int i = 0; i < NurbsCurve.Points.Count; i++)
                {
                    if (memberSmaller[i] == false) continue;

                    Line midLine = new Line(NurbsCurve.Points[i - 1].Location, NurbsCurve.Points[i + 1].Location);
                    Point3d targetPoint = midLine.ClosestPoint(NurbsCurve.Points[i].Location, true);
                    Vector3d moveVector = new Vector3d(targetPoint - NurbsCurve.Points[i].Location);
                    moveVector *= 0.1;

                    if (moveVector.Length < minimumStep) { moveVector.Unitize(); moveVector *= minimumStep; }

                    nurbsPoints.Add(new Tuple<int, Point3d>(i, new Point3d(NurbsCurve.Points[i].Location.X, 0.0, (NurbsCurve.Points[i].Location + moveVector).Z)));
                }

                foreach (Tuple<int, Point3d> pair in nurbsPoints)
                    NurbsCurve.Points.SetPoint(pair.Item1, pair.Item2);

                //update nurbs curve
                BoardCurves = NurbsToBoardCurves();
            }
        }    
    }
}