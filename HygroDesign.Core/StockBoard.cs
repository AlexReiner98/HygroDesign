using System;
using System.Collections.Generic;


namespace BilayerDesign
{
    public class StockBoard : BoardBase
    {
        public double LengthAvailable { get; set; }
        public double SelectedRadius { get; set; }
        public double Thickness { get; set; }
        public double Multiplier { get; set; }
        public List<PanelBoard> DesignBoards { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }

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
            DesignBoards = new List<PanelBoard>();
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