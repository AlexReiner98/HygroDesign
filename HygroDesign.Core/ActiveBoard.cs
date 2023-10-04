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
        public ActiveLayer ActiveLayer { get; set; }
        public List<HMaxel> HMaxels { get; set; }
        public StockBoard StockBoard { get; set; }
        public double DesiredRadius { get; set; }
        public double MoistureChange { get; set; }
        public double Radius { get; set; }
        public Brep ShapedBoard { get; set; }
        public List<int> TrimmedByBrepID { get; set; }
        public ActiveBoard(List<HMaxel> hMaxels, ActiveLayer activeLayer, int id)
        {
            HMaxels = hMaxels;
            ActiveLayer = activeLayer;
            ID = id;
            Attributes = new Dictionary<string, object>();
        }

        public static ActiveBoard DeepCopy(ActiveBoard source, ActiveLayer parent)
        {
            List<HMaxel> hmaxels = new List<HMaxel>();
            for (int i = 0; i < source.HMaxels.Count; i++)
            {
                hmaxels.Add(parent.Bilayer.Panel.HMaxels[source.HMaxels[i].I, source.HMaxels[i].J]);
            }
            ActiveBoard activeBoard = new ActiveBoard(hmaxels, parent, source.ID);
            activeBoard.StockBoard = source.StockBoard;
            activeBoard.DesiredRadius = source.DesiredRadius;
            activeBoard.MoistureChange = source.MoistureChange;
            activeBoard.Radius = source.Radius;
            activeBoard.ShapedBoard = source.ShapedBoard;

            return activeBoard;
        }

        

        public new Species Species
        {
            get
            {
                int bilayerID = ActiveLayer.ID;
                Dictionary<Species, int> speciesCounts = new Dictionary<Species, int>();
                for (int i = 0; i < HMaxels.Count; i++)
                {
                    if (HMaxels[i].Species.Count <= i) continue;
                    Species thisSpecies = HMaxels[i].Species[bilayerID];
                    if (!speciesCounts.ContainsKey(thisSpecies)) speciesCounts.Add(thisSpecies, 1);
                    else speciesCounts[thisSpecies]++;
                }
                int largestCount = 0;
                Species species = null;
                foreach (KeyValuePair<Species, int> pair in speciesCounts)
                {
                    if (pair.Value > largestCount)
                    {
                        largestCount = pair.Value;
                        species = pair.Key;
                    }
                }
                return species;
            }
        }

        public double RadiusParameter
        { get
            {
                double radiusFactor = 0;
                for (int i = 0; i < HMaxels.Count; i++)
                {
                    radiusFactor += HMaxels[i].RadiusParameter;
                }
                return radiusFactor /= HMaxels.Count;
            }
        }

        public double BlendedRadius
        {
            get
            {
                double blendedRadius = 0;
                for (int i = 0; i < HMaxels.Count; i++)
                {
                    blendedRadius += HMaxels[i].BlendedRaidus;
                }
                return blendedRadius /= HMaxels.Count;
            }
        }

        public Interval RowRange
        { get
            {
                double smallest = double.MaxValue;
                double largest = double.MinValue;
                for (int i = 0; i < HMaxels.Count; i++)
                {
                    if (HMaxels[i].RowRange[0] < smallest)
                    {
                        smallest = HMaxels[i].RowRange[0];
                    }
                    if (HMaxels[i].RowRange[1] > largest)
                    {
                        largest = HMaxels[i].RowRange[1];
                    }
                }
                return new Interval(smallest, largest);
            }
        }

        public Interval ColumnRange
        {
            get
            {
                double smallest = double.MaxValue;
                double largest = double.MinValue;
                for (int i = 0; i < HMaxels.Count; i++)
                {
                    if (HMaxels[i].ColumnRange[0] < smallest)
                    {
                        smallest = HMaxels[i].ColumnRange[0];
                    }
                    if (HMaxels[i].ColumnRange[1] > largest)
                    {
                        largest = HMaxels[i].ColumnRange[1];
                    }
                }
                return new Interval(smallest, largest);
            }
        }

        public new double Length
        {
            get
            {
                return RowRange.Length;
            }
        }

        public new double Width
        {
            get
            {
                return RowRange.Length;
            }
        }

        public Point3d ShapedCentroid
        {
            get
            {
                if (ActiveLayer.Bilayer.Panel.Brep == null) throw new Exception("The parent panel's shaped surface must be set before the shaped centroid can be calculated.");
                /*
                double xCoord = RowRange[0] + (Length / 2);
                double yCoord = ColumnRange[0] + (Width / 2);

                return ActiveLayer.Bilayer.Panel.Brep.Faces[0].PointAt(xCoord, yCoord);
                */

                if (ShapedBoard == null) return new Point3d(0,0,0);
                AreaMassProperties results = AreaMassProperties.Compute(ShapedBoard);
                return results.Centroid;
            }
        }

        public double Area
        {
            get
            {
                return Width * Length;
            }
        }

        public double Volume
        {
            get
            {
                return Area * Thickness;
            }
        }

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

        public double PassiveMass
        {
            get
            {
                double passiveVolume = Area * ActiveLayer.Bilayer.PassiveLayer.Thickness;
                double volume = 0;
                if (RhinoDoc.ActiveDoc.ModelUnitSystem == UnitSystem.Millimeters) volume = passiveVolume * 1e-9;
                if (RhinoDoc.ActiveDoc.ModelUnitSystem == UnitSystem.Centimeters) volume = passiveVolume * 1e-6;
                if (RhinoDoc.ActiveDoc.ModelUnitSystem == UnitSystem.Meters) volume = passiveVolume;

                return volume * ActiveLayer.Bilayer.PassiveLayer.Species.Attributes["density"];
            }
        }

        public Rectangle3d Outline {
            get
            {
                return new Rectangle3d(Plane.WorldXY, RowRange, ColumnRange);
            }
        }

        public new double Thickness
        {
            get
            {
                return ActiveLayer.Thickness;
            }
        }
    }
}