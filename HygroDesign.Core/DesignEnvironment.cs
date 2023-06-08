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
        public List<Panel> Panels { get; set; }

        List<StockBoard> allStock = new List<StockBoard>();
        List<PanelBoard> allBoards = new List<PanelBoard>();
        List<Tuple<int, double>> allCurvatures = new List<Tuple<int, double>>();

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
            int index = 0;
            foreach(StockPile stockpile in StockPiles)
            {
                foreach(StockBoard board in stockpile.Boards)
                {
                    allStock.Add(board);
                    foreach(double curvature in board.PotentialCurvatures)
                    {
                        allCurvatures.Add(new Tuple<int,double>(index, curvature));
                    }
                    index++;
                }
            }

            int panelID = 0;
            int boardID = 0;
            foreach(Panel panel in Panels)
            {
                foreach(PanelBoard[] array in panel.Boards)
                {
                    foreach(PanelBoard board in array)
                    {
                        board.panelNumber = panelID;
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
                    if (board.Material.Name == material & boardDicts[material].Item2.Count < boardDicts[material].Item1)
                    {
                        boardDicts[material].Item2.Add(board);
                    }
                }
            }

            //sort material lists by curvature weight
            foreach(string material in boardDicts.Keys)
            {
                List<PanelBoard> panelBoards = boardDicts[material].Item2;
                List<PanelBoard> radiusWeightOrder = panelBoards.OrderByDescending(o => o.RadiusWeight).ToList();
                foreach (PanelBoard board in radiusWeightOrder)
                {
                    //find best closest curvature from flat list
                    double closestDifference = double.MaxValue;
                    int closestID = 0;
                    for(int i = 0; i < allCurvatures.Count;i++)
                    {
                        double currentDifference = Math.Abs(allCurvatures[i].Item2 - board.DesiredRadius);
                        if (currentDifference < closestDifference) { closestDifference = currentDifference; closestID = allCurvatures[i].Item1; }
                    }

                    //assign associated stock board's properties to the panel board
                    board.StockBoard = allStock[closestID];

                    //remove the stockboard from the flattened list and continue
                    allStock.Remove(allStock[closestID]);

                    //update board in panel
                    Panels[board.panelNumber].Boards[board.rowNumber][board.columnNumber] = board;
                }
            }
        }
    }
}