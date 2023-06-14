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
        public double LengthAvailable = 0;
        public double SelectedRadius = 0;
        public double SelectedMoistureChange = 0;

        public StockBoard(string name, Material material, double rtAngle, double length, double width)
        {
            Name = name;
            Material = material;
            RTAngle = rtAngle;
            LengthAvailable = Length = length;
            Width = width;
        }
    }
}