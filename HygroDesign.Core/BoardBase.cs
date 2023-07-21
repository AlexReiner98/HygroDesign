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
        public Species Species = new Species();
        public double Height;
        public double RTAngle;
    }
}