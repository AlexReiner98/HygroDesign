using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class PanelBoard : BoardBase
    {
        private Panel Parent;

        public Interval RowRange;
        public Interval ColumnRange;

        public Polyline Polyline;

        public List<PanelBoard> BoardsAbove = new List<PanelBoard>();
        public List<PanelBoard> BoardsBelow = new List<PanelBoard>();
        public PanelBoard BoardLeft;
        public PanelBoard BoardRight;

        public PanelBoard(Panel parent, Interval rowRange, Interval columnRange)
        {
            Parent = parent;
            RowRange = rowRange;
            ColumnRange = columnRange;
            GenerateRectangle();
        }
        public void GenerateRectangle()
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
        }
    }
}