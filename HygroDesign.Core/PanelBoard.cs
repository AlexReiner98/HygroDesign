using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class PanelBoard : BoardBase
    {
        public Bilayer Parent { get; set; }
        public Interval RowRange { get; set; }
        public Interval ColumnRange { get; set; }
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int PanelNumber { get; set; }
        public Polyline Polyline { get; set; }
        public Point3d Centroid { get; set; }
        public double RadiusParameter { get; set; }
        public double DesiredRadius { get; set; }
        public double RadiusWeight { get; set; }
        public StockBoard StockBoard { get; set; }
        public double Radius { get; set; }
        public double MoistureChange { get; set; }
        public double Error { get; set; }
        public double LongStiffnessFactor { get; set; }
        public double RadStiffnessFactor { get; set; }
        public double RadiusFactor { get; set; }
        public List<Tuple<double,double>> ConvolutionWeights { get; set; }
        public double BlendedRadius { get; set; }
        public List<PanelBoard> ThicknessNeighbors { get; set; }
        public double ThicknessBlendedRadius { get; set; }
        public double ThicknessParameter { get; set; }

        public PanelBoard(Interval rowRange, Interval columnRange, Bilayer parent)
        {
            Parent = parent;
            RowRange = rowRange;
            ColumnRange = columnRange;
            EvaluateBoard();
        }

        public static PanelBoard DeepCopy(PanelBoard source, Bilayer parent)
        {
            Interval rowRange = new Interval(source.RowRange[0], source.RowRange[1]);
            Interval columnRange = new Interval(source.ColumnRange[0], source.ColumnRange[1]);

            PanelBoard newBoard = new PanelBoard(rowRange, columnRange, parent);

            newBoard.Name = source.Name;
            newBoard.Length = source.Length;
            newBoard.Width = source.Width;
            newBoard.Height = source.Height;
            newBoard.MoistureChange = source.MoistureChange;
            newBoard.RTAngle = source.RTAngle;

            newBoard.RowNumber = source.RowNumber;
            newBoard.ColumnNumber = source.ColumnNumber;
            newBoard.PanelNumber = parent.ID;
            newBoard.RadiusParameter = source.RadiusParameter;
            newBoard.DesiredRadius = source.DesiredRadius;
            newBoard.RadiusWeight = source.RadiusWeight;
            newBoard.Species = source.Species;
            newBoard.StockBoard = source.StockBoard;
            newBoard.Radius = source.Radius;
            newBoard.MoistureChange = source.MoistureChange;
            newBoard.Error = source.Error;

            newBoard.LongStiffnessFactor = source.LongStiffnessFactor;
            newBoard.RadStiffnessFactor = source.RadStiffnessFactor;
            newBoard.RadiusFactor = source.RadiusFactor;
            newBoard.ConvolutionWeights = source.ConvolutionWeights;
            newBoard.BlendedRadius = source.BlendedRadius;
            

            return newBoard;
        }
    
        public void EvaluateBoard()
        {
            List<Point3d> points = new List<Point3d>
            {
                Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[0]),
                Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[0]),
                Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[1]),
                Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[1]),
                Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[0])
            };
            Polyline = new Polyline(points);

            Centroid += Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[0]);
            Centroid += Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[0]);
            Centroid += Parent.InitialSurface.PointAt(RowRange[1], ColumnRange[1]);
            Centroid += Parent.InitialSurface.PointAt(RowRange[0], ColumnRange[1]);
            Centroid /= 4;

            Length = RowRange.Length;
            Width = ColumnRange.Length;
        }


    }
}