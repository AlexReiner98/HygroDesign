using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BilayerDesign
{
    public class DesignEnvironment
    {
        public List<StockPile> StockPiles { get; set; }
        public List<Panel> Panels { get; set; }

        List<StockBoard> allStock = new List<StockBoard>();
        List<PanelBoard> allBoards = new List<PanelBoard>();
        List<Tuple<int, double, double>> allCurvatures = new List<Tuple<int, double, double>>();

        public static Material BeechMaterial = new Material("beech",0.0001, 0.0020, 0.0041, 14000, 2280, 1160);
        public static Material SpruceMaterial = new Material("spruce",0.0001, 0.0019, 0.0036, 10000, 800, 450);

        public static Material PassiveMaterial = SpruceMaterial;
        public static double PassiveThickness = 4;

        public DesignEnvironment(List<Panel> panels, List<StockPile> stockPiles)
        {
            Panels = panels;
            StockPiles = stockPiles;
            FlattenLists();
            ApplyStock();
        }

        public void FlattenLists()
        {
            allStock.Clear();
            allBoards.Clear();
            allCurvatures.Clear(); 
            int index = 0;
            foreach(StockPile stockpile in StockPiles)
            {
                foreach(StockBoard board in stockpile.Boards)
                {
                    allStock.Add(board);
                    int moistureIndex = 0;
                    foreach(double curvature in board.PotentialRadii.Keys)
                    {
                        allCurvatures.Add(new Tuple<int,double,double>(index, curvature, stockpile.MoistureChanges[moistureIndex]));
                        moistureIndex++;
                    }
                    index++;
                }
            }

            int panelID = 0;
            int boardID = 0;
            foreach(Panel panel in Panels)
            {
                panel.ID = panelID;
                foreach(PanelBoard[] array in panel.Boards)
                {
                    foreach(PanelBoard board in array)
                    {
                        board.PanelNumber = panelID;
                        allBoards.Add(board);
                    }
                    boardID++;
                    
                }
                panelID++;
            }
        }

        public void ApplyStock()
        {
            
            //sort by material weight
            List<PanelBoard> materialWeightOrder = allBoards.OrderByDescending(o => o.MaterialWeight).ToList();

            //put into new dicts with lenghts limited by the number of stock boards with each material
            Dictionary<string, Tuple<int, List<PanelBoard>>> boardDicts = new Dictionary<string, Tuple<int, List<PanelBoard>>>();
            foreach(StockPile stockPile in StockPiles) boardDicts.Add(stockPile.Material.Name, new Tuple<int, List<PanelBoard>>(stockPile.BoardCount, new List<PanelBoard>()));

            foreach(PanelBoard board in materialWeightOrder)
            {
                foreach(string material in boardDicts.Keys)
                {
                    if (board.DesiredMaterial.Name == material & boardDicts[material].Item2.Count < boardDicts[material].Item1)
                    {
                        boardDicts[material].Item2.Add(board);
                    }
                }
            }

            //sort material lists by curvature weight
            foreach (string material in boardDicts.Keys)
            {
                
                List<PanelBoard> panelBoards = boardDicts[material].Item2;
                
                List<PanelBoard> radiusWeightOrder = panelBoards.OrderByDescending(o => o.RadiusWeight).ToList();
                foreach (PanelBoard board in radiusWeightOrder)
                {
                    
                    //find closest curvature from flat list
                    double closestDifference = double.MaxValue;
                    StockBoard closestStock = null;

                    foreach(StockBoard stockBoard in allStock)
                    {
                        if (stockBoard.Material.Name != material | stockBoard.LengthAvailable < board.Length) continue;

                        foreach(double potentialRadius in stockBoard.PotentialRadii.Keys)
                        {
                            double currentDifference = Math.Abs(potentialRadius - board.DesiredRadius);
                            if(currentDifference < closestDifference)
                            {
                                closestDifference = currentDifference;
                                closestStock = stockBoard;
                                closestStock.SelectedRadius = potentialRadius;
                            }
                        }
                    }
                    closestStock.SelectedMoistureChange = closestStock.PotentialRadii[closestStock.SelectedRadius];
                    closestStock.LengthAvailable -= board.Length;
                    board.SetStockBoard(closestStock);

                    //update board in panel
                    Panels[board.PanelNumber].Boards[board.RowNumber][board.ColumnNumber] = board;
                }
            }
        }
    }
}