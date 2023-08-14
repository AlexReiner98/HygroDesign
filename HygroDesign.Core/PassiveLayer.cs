using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;

namespace BilayerDesign
{
    public class PassiveLayer
    {
        
        public double Thickness { get; set; }
        public Species Species { get; set; }
        public Bilayer Parent { get; set; }
        public Interval XDomain { get; set; }
        public Interval YDomain { get; set; }


        public PassiveLayer( double thickness, Species species, Bilayer parent )
        {
            Thickness = thickness;
            Species = species;
            Parent = parent;
            Update();
        }

        public static PassiveLayer DeepCopy(PassiveLayer source, Bilayer parent)
        {
            PassiveLayer passiveLayer = new PassiveLayer(source.Thickness, source.Species, parent);
            passiveLayer.XDomain = source.XDomain;
            passiveLayer.YDomain = source.YDomain;
            return passiveLayer;
        }

        public void Update()
        {
            double minX = double.MaxValue;
            double maxX = 0;
            double minY = double.MaxValue;
            double maxY = 0;

            foreach(PanelBoard board in Parent.Boards)
            {
                if (board.RowRange[0] < minX) minX = board.RowRange[0];
                if (board.RowRange[1] > maxX) maxX = board.RowRange[1];
                if (board.ColumnRange[0] < minY) minY = board.ColumnRange[0];
                if (board.ColumnRange[1] > maxY) maxY = board.ColumnRange[1];
            }

            minX = minX - Parent.BasePlane.Origin.X;
            maxX = maxX - Parent.BasePlane.Origin.X;
            minY = minY - Parent.BasePlane.Origin.Y;
            maxY = maxY - Parent.BasePlane.Origin.Y;

            XDomain = new Interval(minX, maxX);
            YDomain = new Interval(minY, maxY);
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public double Width { get
            {
                return YDomain.Length;
            } }

        public double Length
        {
            get
            {
                return XDomain.Length;
            }
        }

        public Point3d ShapedCentroid { get
            {
                double xCoord = XDomain[0] + (Length/2);
                double yCoord = YDomain[0] + (Width / 2);

                return Parent.Parent.Surface.PointAt(xCoord, yCoord);
            } 
        }

        public double Area { get
            {
                return Width * Length;
            } }

        public double Volume { get
            {
                return Area * Thickness;
            } }

        public double Mass
        {
            get
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
    }
}