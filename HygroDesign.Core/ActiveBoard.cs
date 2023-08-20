using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class ActiveBoard : WoodElement
    {
        public Bilayer Parent { get; set; }
        public Interval InitialRowRange { get; set; }
        public Interval InitialColumnRange { get; set; }
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public Polyline Polyline { get; set; }
        public double RadiusParameter { get; set; }
        public double DesiredRadius { get; set; }
        public double RadiusWeight { get; set; }
        public StockBoard StockBoard { get; set; }
        public double Radius { get; set; }
        public double MoistureChange { get; set; }
        public double Error { get; set; }
        public double BlendedRadius { get; set; }
        private int RegionCount { get; set; }
        public List<BoardRegion> Regions { get; set; }

        public ActiveBoard(Interval initialRowRange, Interval initialColumnRange, Bilayer parent, int regions)
        {
            Parent = parent;
            InitialRowRange = initialRowRange;
            InitialColumnRange = initialColumnRange;
            RegionCount = regions;
            CreateBoardRegions();
            EvaluateBoard();

        }

        public static ActiveBoard DeepCopy(ActiveBoard source, Bilayer parent)
        {
            Interval initialRowRange = new Interval(source.RowRange[0], source.RowRange[1]);
            Interval initialColumnRange = new Interval(source.ColumnRange[0], source.ColumnRange[1]);

            ActiveBoard newBoard = new ActiveBoard(initialRowRange, initialColumnRange, parent, source.RegionCount);

            newBoard.Name = source.Name;
            newBoard.Height = source.Height;
            newBoard.MoistureChange = source.MoistureChange;
            newBoard.RTAngle = source.RTAngle;

            newBoard.RowNumber = source.RowNumber;
            newBoard.ColumnNumber = source.ColumnNumber;
            newBoard.RadiusParameter = source.RadiusParameter;
            newBoard.DesiredRadius = source.DesiredRadius;
            newBoard.RadiusWeight = source.RadiusWeight;
            newBoard.Species = source.Species;
            newBoard.StockBoard = source.StockBoard;
            newBoard.Radius = source.Radius;
            newBoard.MoistureChange = source.MoistureChange;
            newBoard.Error = source.Error;
            newBoard.BlendedRadius = source.BlendedRadius;
            newBoard.ID = source.ID;

            newBoard.Regions = new List<BoardRegion>();
            foreach(BoardRegion region in source.Regions)
            {
                newBoard.Regions.Add(BoardRegion.DeepCopy(region, newBoard));
            }
            return newBoard;
        }
    
        public void EvaluateBoard()
        {
            List<Point3d> points = new List<Point3d>
            {
                Parent.InitialSurface.PointAt(InitialRowRange[0], InitialColumnRange[0]),
                Parent.InitialSurface.PointAt(InitialRowRange[1], InitialColumnRange[0]),
                Parent.InitialSurface.PointAt(InitialRowRange[1], InitialColumnRange[1]),
                Parent.InitialSurface.PointAt(InitialRowRange[0], InitialColumnRange[1]),
                Parent.InitialSurface.PointAt(InitialRowRange[0], InitialColumnRange[0])
            };
            Polyline = new Polyline(points);

            CenterOfGravity += Parent.InitialSurface.PointAt(InitialRowRange[0], InitialColumnRange[0]);
            CenterOfGravity += Parent.InitialSurface.PointAt(InitialRowRange[1], InitialColumnRange[0]);
            CenterOfGravity += Parent.InitialSurface.PointAt(InitialRowRange[1], InitialColumnRange[1]);
            CenterOfGravity += Parent.InitialSurface.PointAt(InitialRowRange[0], InitialColumnRange[1]);
            CenterOfGravity /= 4;
        }

        private void CreateBoardRegions()
        {
            Regions = new List<BoardRegion>();
            double regionLength = 1 / (double)RegionCount;
            for(int i = 0; i < RegionCount; i++)
            {
                BoardRegion region = new BoardRegion(new Interval(i * regionLength, (i + 1) * regionLength),this);
                region.ID = i; 
                Regions.Add(region);
            }
        }

        public Interval RowRange { get
            {
                double start = double.MaxValue;
                double end = double.MinValue;
                for(int i = 0; i < Regions.Count; i++)
                {
                    if (Regions[i].RowRange[0] < start) start = Regions[i].RowRange[0];
                    if (Regions[i].RowRange[1] > end) end = Regions[i].RowRange[1];
                }
                return new Interval(start, end);
            } 
        }

        public Interval ColumnRange { get
            {
                double start = double.MaxValue;
                double end = double.MinValue;
                for (int i = 0; i < Regions.Count; i++)
                {
                    if (Regions[i].ColumnRange[0] < start) start = Regions[i].ColumnRange[0];
                    if (Regions[i].ColumnRange[1] > end) end = Regions[i].ColumnRange[1];
                }
                return new Interval(start, end);
            } 
        }

        public new double Length { get
            {
                return RowRange.Length;
            }
        }

        public new double Width
        {
            get
            {
                return ColumnRange.Length;
            }
        }

        public Point3d ShapedCentroid { get
            {
                if (Parent.Parent.Surface == null) throw new Exception("The parent panel's shaped surface must be set before the shaped centroid can be calculated.");

                double xCoord = RowRange[0] + (Length / 2);
                double yCoord = ColumnRange[0] + (Width / 2);

                return Parent.Parent.Surface.PointAt(xCoord, yCoord);
            } 
        }

        public double Area { get
            {
                return Width * Length;
            }
        }

        public double Volume { get
            {
                return Area * Parent.ActiveThickness;
            }
        }

        public double Mass { get
            {
                //get rhinodoc units and convert volume to m3
                double volume = 0;
                if (RhinoDoc.ActiveDoc.ModelUnitSystem == UnitSystem.Millimeters) volume = Volume * 1e-9;
                if (RhinoDoc.ActiveDoc.ModelUnitSystem == UnitSystem.Centimeters) volume = Volume * 1e-6;
                if (RhinoDoc.ActiveDoc.ModelUnitSystem == UnitSystem.Meters) volume = Volume;

                //calculate mass in kg
                return volume * Species.Attributes["density"];
            }
        }

        public Surface ShapedBoard { get
            {
                if (Parent.Parent.Surface == null) throw new Exception("Shaped panel surface must be set before shaped board surface cane be retrieved.");
                return Parent.Parent.Surface.Trim(RowRange, ColumnRange);
            }
        }
    }
}