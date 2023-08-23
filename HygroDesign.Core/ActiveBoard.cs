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
        public double RadiusParameter { get; set; }
        public double RadiusWeight { get; set; }
        public double DesiredRadius { get; set; }
        public double Radius { get; set; }
        public double MoistureChange { get; set; }
        public ActiveBoard(List<HMaxel> hMaxels, ActiveLayer activeLayer, int id)
        {
            HMaxels = hMaxels;
            ActiveLayer = activeLayer;
            ID = id;
        }

        public static ActiveBoard DeepCopy(ActiveBoard source, ActiveLayer parent)
        {
            List<HMaxel> hmaxels = new List<HMaxel>();
            for(int i = 0; i < source.HMaxels.Count; i++)
            {
                hmaxels.Add(parent.Bilayer.Panel.HMaxels[source.HMaxels[i].I, source.HMaxels[i].J]);
            }

            return new ActiveBoard(hmaxels, parent, source.ID);
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
                        largest = HMaxels[i].RowRange[0];
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
                        largest = HMaxels[i].ColumnRange[0];
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