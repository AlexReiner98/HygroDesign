using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public abstract class WoodElement
    {
        public string Name { get ; set; }
        public int ID { get; set; }
        public Species Species { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double RTAngle { get; set; }
        public Point3d CenterOfGravity { get; set; }
        public Dictionary<string,object> Attributes { get; set; }
    }
}