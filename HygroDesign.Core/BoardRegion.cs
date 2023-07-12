﻿using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class BoardRegion : BoardBase
    {
        public PanelBoard Parent { get; set; }
        public Interval RowRange { get; set; }
        public Interval ColumnRange { get; set; }
        public Polyline Polyline { get; set; }
        public Point3d Centroid { get; set; }
        public List<BoardRegion> RegionStack { get; set; }
        public double ThicknessBlendedRadius { get; set; }
        public double ThicknessParameter { get; set; }
        public int ID { get; set; }

        public BoardRegion(Interval rowRange, PanelBoard parent)
        {
            Parent = parent;

            double start = Remap(rowRange[0], 0, 1, parent.RowRange[0], parent.RowRange[1]);
            double end = Remap(rowRange[1], 0, 1, parent.RowRange[0], parent.RowRange[1]);
            RowRange = new Interval(start, end);

            ColumnRange = parent.ColumnRange;
            EvaluateBoardRegion();
        }

        public static BoardRegion DeepCopy(BoardRegion source, PanelBoard parent)
        {
           
            Interval rowRange = new Interval(Remap(source.RowRange[0], parent.RowRange[0], parent.RowRange[1],0,1), Remap(source.RowRange[1], parent.RowRange[0], parent.RowRange[1], 0, 1));
            BoardRegion newBoard = new BoardRegion(rowRange, parent);

            newBoard.Length = source.Length;
            newBoard.Width = source.Width;
            newBoard.Height = source.Height;
            newBoard.RegionStack = source.RegionStack;
            newBoard.ThicknessParameter = source.ThicknessParameter;
            newBoard.ThicknessBlendedRadius = source.ThicknessBlendedRadius;
            newBoard.ID = source.ID;

            newBoard.EvaluateBoardRegion();

            return newBoard;
        }
    
        public void EvaluateBoardRegion()
        {
            List<Point3d> points = new List<Point3d>
            {
                Parent.Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[0]),
                Parent.Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[0]),
                Parent.Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[1]),
                Parent.Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[1]),
                Parent.Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[0])
            };
            Polyline = new Polyline(points);

            Centroid += Parent.Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[0]);
            Centroid += Parent.Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[0]);
            Centroid += Parent.Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[1]);
            Centroid += Parent.Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[1]);
            Centroid /= 4;

            Length = RowRange.Length;
            Width = ColumnRange.Length;
        }
        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public new string Name { get
            {
                return Parent.Name;
            } }
        public new Species Species { get
            {
                return Parent.Species;
            }
        }
        public new double RTAngle { get 
            { 
                return Parent.RTAngle; 
            } }
        public double MoistureChange { get
            {
                return Parent.MoistureChange;
            } }

        public double DesiredRadius { get
            {
                return Parent.DesiredRadius;
            } }

        public double BlendedRadius { get
            {
                return Parent.BlendedRadius;
            } }

        public double Radius
        {
            get
            {
                return Parent.Radius;
            }
        }
    }
}