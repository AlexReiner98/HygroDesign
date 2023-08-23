using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;

namespace BilayerDesign
{
    public class PassiveLayer : WoodElement
    {
        private Bilayer Bilayer { get; set; }
        public PassiveLayer(Bilayer bilayer, Species species, double thickness)
        {
            Bilayer = bilayer;
            Species = species;
            Thickness = thickness;
        }

        public static PassiveLayer DeepCopy(PassiveLayer source, Bilayer parent)
        {
           return new PassiveLayer(parent, source.Species, source.Thickness);
        }

        new public int ID { get
            {
                return Bilayer.ID;
            } }

    }
}