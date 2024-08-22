using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BilayerDesign
{
    public class SelectionEnvironment
    {
        public List<Panel> Panels { get; set; }
        public StockPile StockPile { get; set; }
        public List<ActiveBoard> PanelBoards { get; set; }
        public SelectionEnvironment(List<Panel> panels, StockPile stockPile)
        {
            Panels = panels;
            StockPile = stockPile;
            CalculateDesiredRadius();
            ApplyStock();
        }

        public void CalculateDesiredRadius()
        {
            PanelBoards = new List<ActiveBoard>();
            for(int p = 0; p < Panels.Count; p++)
            {
                for(int bi = 0; bi < Panels[p].Bilayers.Count; bi++)
                {
                    for(int bo = 0; bo < Panels[p].Bilayers[bi].ActiveLayer.Boards.Count; bo++)
                    {
                        Panels[p].Bilayers[bi].ActiveLayer.Boards[bo].DesiredRadius = Remap(Panels[p].Bilayers[bi].ActiveLayer.Boards[bo].RadiusParameter, 0, 1, StockPile.MinRadius, StockPile.MaxRadius);
                        ActiveBoard board = Panels[p].Bilayers[bi].ActiveLayer.Boards[bo];
                        board.ActiveLayer.Bilayer = Panels[p].Bilayers[bi];
                        PanelBoards.Add(board);
                    }
                }
            }
        }

        public void ApplyStock()
        {
            //PanelBoards = PanelBoards.OrderByDescending(o => o.RadiusWeight).ToList();
            foreach(Species species in StockPile.StockDictionary.Keys)
            {
                foreach(StockBoard stockboard in StockPile.StockDictionary[species])
                {
                    stockboard.DesignBoards.Clear();
                }
            }

            foreach (ActiveBoard board in PanelBoards)
            {
                Species activeSpecies = board.Species;

                StockBoard closestStock = null;
                double smallestRadDiff = double.MaxValue;
                double selectedMCChange = 0;
                double selectedRadius = 0;
                foreach (StockBoard stockBoard in StockPile.StockDictionary[activeSpecies])
                {
                    if (stockBoard == null || stockBoard.LengthAvailable < board.Length) continue;

                    foreach (double moistureChange in StockPile.MoistureChanges)
                    {
                        double prediction = stockBoard.PotentialRadii[board.ActiveLayer.Bilayer][moistureChange];
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
                closestStock.DesignBoards.Add(board);
            }
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}