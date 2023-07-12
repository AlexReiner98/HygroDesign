using System;
using System.Collections.Generic;
using Rhino;

namespace BilayerDesign
{
    public class StockPile
    {
        public Species Species;
        public List<StockBoard> Boards;
        public List<double> MoistureChanges;
        public List<double> Multipliers;
        public int BoardCount;
        Species PassiveSpecies;
        double PassiveThickness;

        public StockPile(Species material, List<StockBoard> boards, List<double> moistureChanges, List<double> multipliers, Species passiveSpecies, double passiveThickness)
        {
            Species = material;
            Boards = boards;
            BoardCount = boards.Count;
            MoistureChanges = moistureChanges;
            Multipliers = multipliers;
            PassiveSpecies = passiveSpecies;
            PassiveThickness = passiveThickness;
        }

        public static StockPile DeepCopy(StockPile source)
        {
            Species material = source.Species;
            Species passiveMaterial = source.PassiveSpecies;
            double passiveThickness = source.PassiveThickness;

            List<StockBoard> boards = new List<StockBoard>();
            List<double> moistures = new List<double>();
            List<double> multipliers = new List<double>();

            foreach(StockBoard board in source.Boards) boards.Add(StockBoard.DeepCopy(board));
            foreach (double moisture in source.MoistureChanges) moistures.Add(moisture);
            foreach (double multiplier in source.Multipliers) multipliers.Add(multiplier);

            return new StockPile(material, boards, moistures, multipliers,passiveMaterial,passiveThickness);
        }

        
    }
}