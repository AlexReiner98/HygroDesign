using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Rhino;

namespace BilayerDesign
{
    public class StockPile
    {
        public PredictionBase PredictionBase { get; set; }
        public Dictionary<Species, List<StockBoard>> StockDictionary { get; set; }
        public List<Panel> Panels { get; set; }
        public List<StockBoard> StockBoards { get; set; }
        public List<double> MoistureChanges { get; set; }
        public double MaxRadius { get; set; }
        public double MinRadius { get; set; }

        public StockPile(List<Panel> panels, List<StockBoard> stockBoards, List<double> moistureChanges, PredictionBase predictionBase)
        {
            Panels = panels;

            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].ID = i;
            }

            StockBoards = stockBoards;
            MoistureChanges = moistureChanges;
            PredictionBase = predictionBase;
            StockDictionary = new Dictionary<Species, List<StockBoard>>();

            AnalyzeInputs();
        }

        public void AnalyzeInputs()
        {
            MaxRadius = double.MinValue;
            MinRadius = double.MaxValue;

            for (int s = 0; s < StockBoards.Count; s++)
            {
                if (!StockDictionary.ContainsKey(StockBoards[s].Species)) StockDictionary.Add(StockBoards[s].Species, new List<StockBoard>());
                StockDictionary[StockBoards[s].Species].Add(StockBoards[s]);

                for (int p = 0; p <  Panels.Count; p++)
                {
                    for(int bi = 0; bi < Panels[p].Bilayers.Count; bi++)
                    {
                        StockBoards[s].PotentialRadii.Add(Panels[p].Bilayers[bi], new  Dictionary<double, double>());
                        for (int m = 0; m < MoistureChanges.Count; m++)
                        {
                            double potentialRadius = PredictionBase.Predict(StockBoards[s], Panels[p].Bilayers[bi], MoistureChanges[m]);
                            StockBoards[s].PotentialRadii[Panels[p].Bilayers[bi]].Add(MoistureChanges[m], potentialRadius);

                            if(potentialRadius > MaxRadius) MaxRadius = potentialRadius;
                            if(potentialRadius < MinRadius) MinRadius = potentialRadius;
                        }
                    }
                }
            }

        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }

    public abstract class PredictionBase
    {
        public abstract double Predict(StockBoard board, Bilayer bilayer, double moistureChange);
    }
}