using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class StockBoard : BoardBase
    {

        public Dictionary<double, double> PotentialRadii = new Dictionary<double, double>();
        public bool Available = true;
        public double SelectedRadius = 0;
        public double SelectedMoistureChange = 0;

        public StockBoard(string name, Material material, double rtAngle, double length)
        {
            Name = name;
            Material = material;
            RTAngle = rtAngle;
            Length = length;
            Height = 20;
        }
    }
}