using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BilayerDesign
{
    public class DesignEnvironment
    {
        public List<Panel> Panels { get; set; }

        public List<StockBoard> StockBoards = new List<StockBoard>();
        private List<PanelBoard> PanelBoards = new List<PanelBoard>();

        private Dictionary<Species, List<StockBoard>> StockDictionary = new Dictionary<Species, List<StockBoard>>();

        private List<double> ActiveThicknesses = new List<double>();
        private List<Species> ActiveSpecies = new List<Species>();
        private List<double> PassiveThicknesses = new List<double>();
        private List<Species> PassiveSpecies = new List<Species>();
        private List<double> MoistureChanges = new List<double>();

        private double MaxRadius = 0;
        private double MinRadius = double.MaxValue;

        public DesignEnvironment(List<Panel> panels, List<StockBoard> stockBoards, List<double> moistureChanges)
        {
            Panels = panels;

            for(int i = 0; i < Panels.Count; i++)
            {
                Panels[i].ID = i;
            }

            StockBoards = stockBoards;
            MoistureChanges = moistureChanges;
            AnalyzeInputs();
            CalculateDesiredRadius();
            ApplyStock();
        }

        public void AnalyzeInputs()
        {
            PanelBoards.Clear();

            foreach(Panel panel in Panels)
            {
                foreach(Bilayer bilayer in panel.Bilayers)
                {
                    foreach(PanelBoard board in bilayer.Boards)
                    {
                        PanelBoards.Add(board);

                        if (!ActiveSpecies.Contains(board.Species)) ActiveSpecies.Add(board.Species);
                    }

                    if (!ActiveThicknesses.Contains(bilayer.ActiveThickness)) ActiveThicknesses.Add(bilayer.ActiveThickness);
                    if (!PassiveThicknesses.Contains(bilayer.PassiveLayer.Thickness)) PassiveThicknesses.Add(bilayer.PassiveLayer.Thickness);
                    if (!PassiveSpecies.Contains(bilayer.PassiveLayer.Species)) PassiveSpecies.Add(bilayer.PassiveLayer.Species);
                }
            }

            foreach(StockBoard stockBoard in StockBoards)
            {

                //fill in stockboard possible radii
                foreach(double activeThickness in  ActiveThicknesses)
                {
                    if (activeThickness > stockBoard.Thickness) continue;
                    stockBoard.PotentialRadii.Add(activeThickness, new Dictionary<double, Dictionary<Species, Dictionary<double, double>>>());
                    
                    foreach(double passiveThickness in PassiveThicknesses)
                    {
                        stockBoard.PotentialRadii[activeThickness].Add(passiveThickness, new Dictionary<Species, Dictionary<double, double>>());
                        foreach (Species passiveSpecies in PassiveSpecies)
                        {
                            stockBoard.PotentialRadii[activeThickness][passiveThickness].Add(passiveSpecies, new Dictionary<double, double>());
                            foreach (double moistureChange in MoistureChanges)
                            {
                                double potentialRadius = Timoshenko(stockBoard.RTAngle, moistureChange, stockBoard.Species, passiveSpecies, activeThickness, passiveThickness, stockBoard.Multiplier);
                                stockBoard.PotentialRadii[activeThickness][passiveThickness][passiveSpecies].Add(moistureChange, potentialRadius);

                                //update radius range
                                if(potentialRadius > MaxRadius) MaxRadius = potentialRadius;
                                if(potentialRadius < MinRadius) MinRadius = potentialRadius;
                            }
                        }
                    }
                }
                if (!StockDictionary.ContainsKey(stockBoard.Species)) StockDictionary.Add(stockBoard.Species,new List<StockBoard>());
                StockDictionary[stockBoard.Species].Add(stockBoard);
            }
        }

        public void CalculateDesiredRadius()
        {
            
            foreach(PanelBoard panelBoard in PanelBoards)
            {
                panelBoard.DesiredRadius = Remap(panelBoard.RadiusParameter, 0, 1, MinRadius, MaxRadius);
            }
        }


        public void ApplyStock()
        {
            PanelBoards.OrderByDescending(o => o.RadiusWeight).ToList();

            foreach(PanelBoard board in PanelBoards)
            {
                Species activeSpecies = board.Species;
                double activeThickness = board.Parent.ActiveThickness;
                double passiveThickness = board.Parent.PassiveLayer.Thickness;
                Species passiveSpecies = board.Parent.PassiveLayer.Species;

                StockBoard closestStock = null;
                double smallestRadDiff = double.MaxValue;
                double selectedMCChange = 0;
                double selectedRadius = 0;
                foreach(StockBoard stockBoard in StockDictionary[activeSpecies])
                {
                    if (stockBoard.LengthAvailable < board.Length) continue;
                    if (stockBoard.PotentialRadii[activeThickness][passiveThickness][passiveSpecies] == null) continue;

                    foreach(double moistureChange in MoistureChanges)
                    {
                        double prediction = stockBoard.PotentialRadii[activeThickness][passiveThickness][passiveSpecies][moistureChange];
                        double difference = Math.Abs(prediction - board.DesiredRadius);
                        if(difference < smallestRadDiff)
                        {
                            smallestRadDiff = difference;
                            closestStock = stockBoard;
                            selectedMCChange = moistureChange;
                            selectedRadius = prediction;
                        }
                    }
                }
                board.StockBoard = closestStock;
                board.Radius = selectedRadius;
                board.MoistureChange = selectedMCChange;
                board.RTAngle = closestStock.RTAngle;
                closestStock.LengthAvailable -= board.Length;
                closestStock.DesignBoards.Add(board);
            }
        }

        public static double Timoshenko(double rtAngle, double wmcc, Species activeMaterial, Species passiveMaterial, double activeThickness, double passiveThickness, double timError)
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

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }
}