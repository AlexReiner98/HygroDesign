using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BilayerDesign
{
    public class ApplicationEnvironment
    {
        public List<Panel> Panels { get; set; }
        public StockPile StockPile { get; set; }
        public ApplicationEnvironment(List<Panel> panels, StockPile stockPile)
        {
            Panels = panels;
            StockPile = stockPile;

            CalculateDesiredRadius();
            ApplyStock();
        }
        public void CalculateDesiredRadius()
        {
            for(int p = 0; p < Panels.Count; p++)
            {
                for(int bi = 0; bi < Panels[p].Bilayers.Count; bi++)
                {
                    for(int bo = 0; bo < Panels[p].Bilayers[bi].Boards.Count; bo++)
                    {
                        Panels[p].Bilayers[bi].Boards[bo].DesiredRadius = Remap(Panels[p].Bilayers[bi].Boards[bo].RadiusParameter, 0, 1, StockPile.MinRadius, StockPile.MaxRadius);
                    }
                }
            }
        }

        public void ApplyStock()
        {
            List<PanelBoard> sortedBoards = StockPile.PanelBoards.OrderByDescending(o => o.RadiusWeight).ToList();

            foreach (PanelBoard board in sortedBoards)
            {
                Species activeSpecies = board.Species;
                double activeThickness = board.Parent.ActiveThickness;
                double passiveThickness = board.Parent.PassiveLayer.Thickness;
                Species passiveSpecies = board.Parent.PassiveLayer.Species;

                StockBoard closestStock = null;
                double smallestRadDiff = double.MaxValue;
                double selectedMCChange = 0;
                double selectedRadius = 0;
                foreach (StockBoard stockBoard in StockPile.StockDictionary[activeSpecies])
                {
                    if (stockBoard.LengthAvailable < board.Length) continue;
                    if (stockBoard.PotentialRadii[activeThickness][passiveThickness][passiveSpecies] == null) continue;

                    foreach (double moistureChange in StockPile.MoistureChanges)
                    {
                        double prediction = stockBoard.PotentialRadii[activeThickness][passiveThickness][passiveSpecies][moistureChange];
                        double difference = Math.Abs(prediction - board.DesiredRadius);
                        if (difference < smallestRadDiff)
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

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}