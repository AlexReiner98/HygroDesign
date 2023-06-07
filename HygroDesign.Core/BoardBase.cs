using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public abstract class BoardBase
    {
        public string Name;
        public Material Material;
        public double Length;
        public double Width;
        public double Height;
        public double RTAngle;
    }
}