using System;
using System.Collections.Generic;


namespace BilayerDesign
{
    public class StockBoard : WoodElement
    {
         
        public double MoistureChange { get; set; }
        public List<ActiveBoard> DesignBoards { get; set; }
        public Dictionary<Bilayer, Dictionary<double, double>> PotentialRadii { get; set; }
        
        
        public StockBoard(string name, Species species, double rtAngle, double thickness, double length, double width, Dictionary<string, object> attributes)
        {
            Name = name;
            Length = length;
            Species = species;
            RTAngle = rtAngle;
            Thickness = thickness;
            Width = width;
            Attributes = attributes;
            DesignBoards = new List<ActiveBoard>();
            PotentialRadii = new Dictionary<Bilayer, Dictionary<double, double>>();
        }

        public static StockBoard DeepCopy(StockBoard source)
        {
            string name = source.Name;
            Species species = source.Species;
            double rtAngle = source.RTAngle;
            double height = source.Thickness;
            double length = source.Length;
            double width = source.Width;
            Dictionary<string, object> attributes = source.Attributes;
            StockBoard stockBoard = new StockBoard(name, species, rtAngle, height, length, width, attributes);
            stockBoard.DesignBoards = source.DesignBoards;
            return stockBoard;
        }

        public double LengthAvailable
        {
            get
            {
                double total = 0;
                for(int i = 0; i < DesignBoards.Count; i++)
                {
                    total += DesignBoards[i].Length;
                }
                return Length - total;
            }
        }
    }
}