using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class StockPile
    {
        public List<StockBoard> Boards;
        public List<double> MoistureChanges;
        public int SpruceBoardCount = 0;
        public int BeechBoardCount = 0;

        public StockPile(List<StockBoard> boards, List<double> moistureChanges)
        {
            Boards = boards;
            MoistureChanges = moistureChanges;
            EvaluateStockPile();
        }

        private void EvaluateStockPile()
        {
            for(int i = 0; i < Boards.Count; i++)
            {
                if (Boards[i].Material == DesignEnvironment.SpruceMaterial) SpruceBoardCount++;
                if (Boards[i].Material == DesignEnvironment.BeechMaterial) BeechBoardCount++;


                Boards[i].PotentialCurvatures.Clear();
                foreach(double moistureChange in MoistureChanges)
                {
                    double curvature = DesignEnvironment.Timoshenko(Boards[i].RTAngle, moistureChange, Boards[i].Material, DesignEnvironment.PassiveMaterial, Boards[i].Height, DesignEnvironment.PassiveThickness, 1.0);
                    Boards[i].PotentialCurvatures.Add(curvature);
                }
            }
        }
    }
}