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
        public Dictionary<string, double> Attributes { get; set; }
        public Dictionary<Bilayer, Dictionary<double, double>> PotentialRadii { get; set; }
        
        
        public StockBoard(string name, Species species, double rtAngle, double thickness, double length, double width, Dictionary<string, double> attributes)
        {
            Name = name;
            Species = species;
            RTAngle = rtAngle;
            LengthAvailable = Length = length;
            Thickness = thickness;
            Width = width;
            Attributes = attributes;
            DesignBoards = new List<PanelBoard>();
            PotentialRadii = new Dictionary<Bilayer, Dictionary<double, double>>();
        }

        public static StockBoard DeepCopy(StockBoard source)
        {
            string name = source.Name;
            Species species = source.Species;
            double rtAngle = source.RTAngle;
            double thickness = source.Thickness;
            double length = source.Length;
            double width = source.Width;
            Dictionary<string, double> attributes = source.Attributes;
            StockBoard stockBoard = new StockBoard(name, species, rtAngle, thickness, length, width, attributes);
            stockBoard.LengthAvailable = source.LengthAvailable;
            stockBoard.DesignBoards = source.DesignBoards;
            return stockBoard;
        }
    }
}