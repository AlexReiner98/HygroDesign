using System;
using System.Collections.Generic;


namespace BilayerDesign
{
    public class StockBoard : BoardBase
    {
        public double LengthAvailable = 0;
        public double SelectedRadius = 0;
        public double Thickness = 0;
        public double Multiplier = 0;
        public List<PanelBoard> DesignBoards = new List<PanelBoard>();
        public double Length = 0;
        public double Width = 0;

        //first key is active thickness, second is passive thickness, third is passive material, fourth is moisture change
        public Dictionary<double, Dictionary<double, Dictionary<Species, Dictionary<double, double>>>> PotentialRadii = new Dictionary<double, Dictionary<double, Dictionary<Species, Dictionary<double, double>>>>();
        public StockBoard(string name, Species species, double rtAngle, double thickness, double length, double width, double multiplier)
        {
            Name = name;
            Species = species;
            RTAngle = rtAngle;
            LengthAvailable = Length = length;
            Thickness = thickness;
            Width = width;
            Multiplier = multiplier;
        }

        public static StockBoard DeepCopy(StockBoard source)
        {
            string name = source.Name;
            Species species = source.Species;
            double rtAngle = source.RTAngle;
            double thickness = source.Thickness;
            double length = source.Length;
            double width = source.Width;
            double multiplier = source.Multiplier;
            StockBoard stockBoard = new StockBoard(name, species, rtAngle, thickness, length, width, multiplier);
            stockBoard.LengthAvailable = source.LengthAvailable;
            stockBoard.DesignBoards = source.DesignBoards;
            return stockBoard;
        }
    }
}