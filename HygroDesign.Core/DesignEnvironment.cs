using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class DesignEnvironment
    {
        List<StockPile> StockPiles { get; set; }
        List<Panel> Panels { get; set; }

        public static Material BeechMaterial = new Material(0.0001, 0.002, 0.0041, 14000, 2280, 1160);
        public static Material SpruceMaterial = new Material(0.0001, 0.0019, 0.0036, 10000, 800, 450);

        public static Material PassiveMaterial = SpruceMaterial;
        public static double PassiveThickness = 4;

        public DesignEnvironment(List<Panel> panels, List<StockPile> stockPiles)
        {
            Panels = panels;
            StockPiles = stockPiles;
        }

        public static double Timoshenko(double rtAngle, double wmcc, Material activeMaterial, Material passiveMaterial, double activeThickness, double passiveThickness, double timError)
        {
            double h1 = passiveThickness;
            double h2 = activeThickness;
            double h = h1 + h2;
            double m = h1 / h2;
            double e1 = passiveMaterial.LElasticModulus;
            double e2 = GrainAngleInterpolation(rtAngle, activeMaterial.RElasticModulus, activeMaterial.TElasticModulus);
            double n = e1 / e2;
            double a1 = passiveMaterial.LExpansion;
            double a2 = GrainAngleInterpolation(rtAngle, activeMaterial.RExpansion, activeMaterial.TExpansion);
            double deltaAlpha = a2 - a1;
            double kValue = 6 * Math.Pow(1.0 + m,2) / (3 * Math.Pow(1 + m,2) + (1 + m * n) * Math.Pow(m,2) + (1 / (m * n)));
            double curvature = kValue * ((wmcc * deltaAlpha) / h);
            return 1 / (curvature * timError);
        }

        public static double GrainAngleInterpolation(double rtAngle, double eR, double eT)
        {
            double angleConv = RhinoMath.ToRadians(rtAngle);
            double angleT = Math.Cos(angleConv);
            double angleR = Math.Sin(angleConv);
            double eActive = eT * Math.Pow(angleT,2) + eR * Math.Pow(angleR,2);
            return eActive;
        }
    }
}