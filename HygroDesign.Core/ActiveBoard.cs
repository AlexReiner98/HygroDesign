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
            for(int i = 0; i < source.HMaxels.Count; i++)
            {
                hmaxels.Add(parent.Bilayer.Panel.HMaxels[source.HMaxels[i].I, source.HMaxels[i].J]);
            }
            ActiveBoard activeBoard = new ActiveBoard(hmaxels, parent, source.ID);
            activeBoard.StockBoard = source.StockBoard;
            activeBoard.DesiredRadius = source.DesiredRadius;
            activeBoard.MoistureChange = source.MoistureChange;
            activeBoard.Radius = source.Radius;

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
                foreach(KeyValuePair<Species, int> pair in speciesCounts)
                {
                    if(pair.Value > largestCount)
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
                for(int i = 0; i < HMaxels.Count; i++)
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
                for(int i = 0; i < HMaxels.Count; i++)
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

        public Rectangle3d Outline { 
            get
            {
                return new Rectangle3d(Plane.WorldXY, RowRange, ColumnRange);
            } 
        }
    }
}