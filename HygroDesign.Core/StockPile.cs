using System;
using System.Collections.Generic;
using Rhino;

namespace BilayerDesign
{
    public class StockPile
    {
        public PredictionBase PredictionBase { get; set; }
        public Dictionary<Species, List<StockBoard>> StockDictionary = new Dictionary<Species, List<StockBoard>>();
        public List<Panel> Panels { get; set; }
        public List<StockBoard> StockBoards { get; set; }
        public List<PanelBoard> PanelBoards { get; set; }
        public List<double> ActiveThicknesses { get; set; }
        public List<Species> ActiveSpecies { get; set; }
        private List<double> PassiveThicknesses { get; set; }
        public List<Species> PassiveSpecies { get; set; }
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

            AnalyzeInputs();
        }

        public void AnalyzeInputs()
        {
            PanelBoards = new List<PanelBoard>();
            ActiveSpecies = new List<Species>();
            PassiveSpecies = new List<Species>();
            ActiveThicknesses = new List<double>();
            PassiveThicknesses = new List<double>();

            MaxRadius = double.MinValue;
            MinRadius = double.MaxValue;

            foreach (Panel panel in Panels)
            {
                foreach (Bilayer bilayer in panel.Bilayers)
                {
                    foreach (PanelBoard board in bilayer.Boards)
                    {
                        PanelBoards.Add(board);
                        if (!ActiveSpecies.Contains(board.Species)) ActiveSpecies.Add(board.Species);
                    }

                    if (!ActiveThicknesses.Contains(bilayer.ActiveThickness)) ActiveThicknesses.Add(bilayer.ActiveThickness);
                    if (!PassiveThicknesses.Contains(bilayer.PassiveLayer.Thickness)) PassiveThicknesses.Add(bilayer.PassiveLayer.Thickness);
                    if (!PassiveSpecies.Contains(bilayer.PassiveLayer.Species)) PassiveSpecies.Add(bilayer.PassiveLayer.Species);
                }
            }

            foreach (StockBoard stockBoard in StockBoards)
            {
                foreach (double activeThickness in ActiveThicknesses)
                {
                    if (activeThickness > stockBoard.Thickness) continue;
                    stockBoard.PotentialRadii.Add(activeThickness, new Dictionary<double, Dictionary<Species, Dictionary<double, double>>>());

                    foreach (double passiveThickness in PassiveThicknesses)
                    {
                        stockBoard.PotentialRadii[activeThickness].Add(passiveThickness, new Dictionary<Species, Dictionary<double, double>>());
                        foreach (Species passiveSpecies in PassiveSpecies)
                        {
                            stockBoard.PotentialRadii[activeThickness][passiveThickness].Add(passiveSpecies, new Dictionary<double, double>());
                            foreach (double moistureChange in MoistureChanges)
                            {
                                double potentialRadius = PredictionBase.Predict(stockBoard, passiveSpecies, moistureChange, activeThickness, passiveThickness);
                                stockBoard.PotentialRadii[activeThickness][passiveThickness][passiveSpecies].Add(moistureChange, potentialRadius);

                                if (potentialRadius > MaxRadius) MaxRadius = potentialRadius;
                                if (potentialRadius < MinRadius) MinRadius = potentialRadius;
                            }
                        }
                    }
                }
                if (!StockDictionary.ContainsKey(stockBoard.Species)) StockDictionary.Add(stockBoard.Species, new List<StockBoard>());
                StockDictionary[stockBoard.Species].Add(stockBoard);
            }
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }

    public abstract class PredictionBase
    {
        public abstract double Predict(StockBoard board, Species passiveSpecies, double moistureChange, double activeThickness, double passiveThickness);
    }

    
}