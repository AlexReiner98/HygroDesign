using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class HMaxel
    {
        Panel Panel { get; set; }
        public Interval RowRange { get; set; }
        public Interval ColumnRange { get; set; }
        public List<Bilayer> Bilayers { get; set; }
        public double Height { get; set; }
        public int I { get; set; }
        public int J { get; set; }

        public HMaxel(Interval rowRange, Interval columnRange, Panel panel, int i, int j)
        {
            RowRange = rowRange;
            ColumnRange = columnRange;
            Panel = panel;
            I = i;
            J = j;
        }

        public static HMaxel DeepCopy(HMaxel source, Panel parent)
        {
            HMaxel hmaxel = new HMaxel(source.RowRange, source.ColumnRange, parent, source.I, source.J);
            
            return hmaxel;
        }

        public double Length {
            get
            {
                return RowRange.Length;
            }
        }

        public double Width
        {
            get
            {
                return ColumnRange.Length;
            }
        }

        public Rectangle3d Outline
        {
            get
            {
                return new Rectangle3d(Plane.WorldXY, RowRange, ColumnRange);
            }
        }
    }
}