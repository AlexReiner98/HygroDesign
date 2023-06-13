using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class PanelBoard : BoardBase
    {
        private Panel Parent { get; set; }

        public Interval RowRange { get; set; }
        public Interval ColumnRange { get; set; }

        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int PanelNumber { get; set; }

        public Polyline Polyline { get; set; }
        public Point3d Centroid { get; set; }

        public double DesiredRadius { get; set; }
        public double RadiusWeight { get; set; }
        public Material DesiredMaterial { get; set; }
        public double MaterialWeight { get; set; }

        public StockBoard StockBoard { get; set; }
        public double Radius { get; set; }
        public double MoistureChange { get; set; }
        public double Error { get; set; }

        public List<PanelBoard> BoardsAbove { get; set; }
        public List<PanelBoard> BoardsBelow { get; set; }
        public PanelBoard BoardLeft { get; set; }
        public PanelBoard BoardRight { get; set; }

        public PanelBoard(Interval rowRange, Interval columnRange, Panel parent)
        {
            Parent = parent;
            RowRange = rowRange;
            Length = rowRange.Length;
            ColumnRange = columnRange;
            EvaluateBoard();
        }

        public static PanelBoard DeepCopy(PanelBoard source, Panel parent)
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
            newBoard.DesiredRadius = source.DesiredRadius;
            newBoard.RadiusWeight = source.RadiusWeight;
            newBoard.Material = source.Material;
            newBoard.MaterialWeight = source.MaterialWeight;
            newBoard.StockBoard = source.StockBoard;
            newBoard.Radius = source.Radius;
            newBoard.MoistureChange = source.MoistureChange;
            newBoard.Error = source.Error;

            

            return newBoard;
        }
    
        public void EvaluateBoard()
        {
            List<Point3d> points = new List<Point3d>
            {
                Parent.Surface.PointAt(RowRange[0], ColumnRange[0]),
                Parent.Surface.PointAt(RowRange[1], ColumnRange[0]),
                Parent.Surface.PointAt(RowRange[1], ColumnRange[1]),
                Parent.Surface.PointAt(RowRange[0], ColumnRange[1]),
                Parent.Surface.PointAt(RowRange[0], ColumnRange[0])
            };
            Polyline = new Polyline(points);

            Centroid += Parent.Surface.PointAt(RowRange[0], ColumnRange[0]);
            Centroid += Parent.Surface.PointAt(RowRange[1], ColumnRange[0]);
            Centroid += Parent.Surface.PointAt(RowRange[1], ColumnRange[1]);
            Centroid += Parent.Surface.PointAt(RowRange[0], ColumnRange[1]);
            Centroid /= 4;
        }

        public void SetStockBoard(StockBoard stockBoard)
        {
            StockBoard = stockBoard;
            Name = stockBoard.Name;
            Radius = stockBoard.SelectedRadius;
            MoistureChange = stockBoard.SelectedMoistureChange;
            Error = Math.Abs(Radius - DesiredRadius) / (Radius + DesiredRadius) * 100;
        }
    }
}