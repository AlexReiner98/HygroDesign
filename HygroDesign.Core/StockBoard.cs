using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class StockBoard : BoardBase
    {

        public List<double> PotentialCurvatures = new List<double>();

        public StockBoard(string name, Material material, double rtAngle, double length, double width, double height)
        {
            Name = name;
            Material = material;
            RTAngle = rtAngle;
            Length = length;
            Width = width;
            Height = height;
        }
    }
}