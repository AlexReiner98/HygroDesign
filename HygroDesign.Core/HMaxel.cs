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
        public Panel Panel { get; set; }
        public Interval RowRange { get; set; }
        public Interval ColumnRange { get; set; }
        public List<PassiveLayer> PassiveLayers { get; set; }
        public List<ActiveBoard> ActiveBoards { get; set; }
        public double Height { get; set; }
        public double RadiusParameter { get; set; }
        public double BlendedRaidus { get; set; }
        public List<Species> Species { get; set; }
        public int I { get; set; }
        public int J { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public Brep ShapedHMaxel { get; set; }


        public HMaxel(Interval rowRange, Interval columnRange, Panel panel, int i, int j)
        {
            RowRange = rowRange;
            ColumnRange = columnRange;
            Panel = panel;
            I = i;
            J = j;
            PassiveLayers = new List<PassiveLayer>();
            ActiveBoards = new List<ActiveBoard>();
            Attributes = new Dictionary<string, object>();
            Species = new List<Species>();
            RadiusParameter = 0;
            BlendedRaidus = 0;
        }

        public static HMaxel DeepCopy(HMaxel source, Panel parent)
        {
            HMaxel hmaxel = new HMaxel(source.RowRange, source.ColumnRange, parent, source.I, source.J);
            List<PassiveLayer> passiveLayers = new List<PassiveLayer>();
            List<ActiveBoard> activeBoards = new List<ActiveBoard>();

            for(int i = 0; i < source.PassiveLayers.Count; i++)
            {
                if (parent.Bilayers.Count == 0) continue;
                passiveLayers.Add(PassiveLayer.DeepCopy(source.PassiveLayers[i], parent.Bilayers[source.PassiveLayers[i].ID]));
            }
            for(int i = 0; i < source.ActiveBoards.Count; i++)
            {
                if (parent.Bilayers.Count == 0) continue;
                activeBoards.Add(ActiveBoard.DeepCopy(source.ActiveBoards[i], parent.Bilayers[source.ActiveBoards[i].ActiveLayer.Bilayer.ID].ActiveLayer));
            }
            hmaxel.Height = source.Height;
            hmaxel.RadiusParameter = source.RadiusParameter;
            hmaxel.BlendedRaidus = source.BlendedRaidus;
            hmaxel.Species = source.Species;
            hmaxel.PassiveLayers = passiveLayers;
            hmaxel.ActiveBoards = activeBoards;
            hmaxel.ShapedHMaxel = source.ShapedHMaxel;
            return hmaxel;
        }

        public double DesiredRadius
        {
            get
            {
                double desiredRadius = 0;
                for(int i = 0; i < ActiveBoards.Count; i++)
                {
                    desiredRadius += ActiveBoards[i].DesiredRadius;
                }
                return desiredRadius / ActiveBoards.Count;
            }
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

        public double X
        {
            get
            {
                return RowRange[0] + (Length / 2);
            }
        }

        public double Y
        {
            get
            {
                return ColumnRange[0] + (Width / 2);
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