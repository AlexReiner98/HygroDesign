using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class StockPile
    {
        public Material Material;
        public List<StockBoard> Boards;
        public List<double> MoistureChanges;
        public int BoardCount;

        public StockPile(Material material, List<StockBoard> boards, List<double> moistureChanges)
        {
            Material = material;
            Boards = boards;
            BoardCount = boards.Count;
            MoistureChanges = moistureChanges;
            EvaluateStockPile();
        }

        public static StockPile DeepCopy(StockPile source)
        {
            Material material = source.Material;

            List<StockBoard> boards = new List<StockBoard>();
            List<double> moistures = new List<double>();

            foreach(StockBoard board in source.Boards) boards.Add(StockBoard.DeepCopy(board));
            foreach (double moisture in source.MoistureChanges) moistures.Add(moisture);

            return new StockPile(material, boards, moistures);
        }

        private void EvaluateStockPile()
        {
            for(int i = 0; i < Boards.Count; i++)
            {
                Boards[i].PotentialRadii.Clear();
                foreach(double moistureChange in MoistureChanges)
                {
                    
                    double curvature = Timoshenko(Boards[i].RTAngle, moistureChange, Material, DesignEnvironment.PassiveMaterial, 20, DesignEnvironment.PassiveThickness, 1.0);
                    Boards[i].PotentialRadii.Add(curvature,moistureChange);
                }
            }
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
            double kValue = 6 * Math.Pow(1.0 + m, 2) / (3 * Math.Pow(1 + m, 2) + (1 + m * n) * Math.Pow(m, 2) + (1 / (m * n)));
            double curvature = kValue * ((wmcc * deltaAlpha) / h);
            return 1 / (curvature * timError);
        }

        public static double GrainAngleInterpolation(double rtAngle, double eR, double eT)
        {
            double angleConv = RhinoMath.ToRadians(rtAngle);
            double angleT = Math.Cos(angleConv);
            double angleR = Math.Sin(angleConv);
            double eActive = eT * Math.Pow(angleT, 2) + eR * Math.Pow(angleR, 2);
            return eActive;
        }
    }
}