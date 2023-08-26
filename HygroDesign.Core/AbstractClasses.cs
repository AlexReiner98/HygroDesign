using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public abstract class WoodAssembly
    {
        public string Name { get ; set; }
        public int ID { get; set; }
        public Species Species { get; set; }
        public double Thickness { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public Point3d Centroid { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
    }

    public abstract class WoodElement
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public Species Species { get; set; }
        public double Thickness { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double RTAngle { get; set; }
        public Point3d Centroid { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
    }

    public abstract class PredictionEngine
    {
        public abstract double Predict(StockBoard board, Bilayer bilayer, double moistureChange);
    }

    public abstract class ConvolutionEngine
    {
        public abstract Panel Convolution(Panel panel);
    }

    public class HMaxelConvolution : ConvolutionEngine
    {
        double Tangential { get; set; }
        double Longitudinal { get; set; }
        double RadiusMax { get; set; }
        double StiffnessInfluence { get; set; }
        double ThicknessInfluence { get; set; }
        public HMaxelConvolution(double tangential, double longitudinal, double radiusMax, double stiffnessInfluence, double thicknessInfluence)
        {
            Tangential = tangential;
            Longitudinal = longitudinal;
            RadiusMax = radiusMax;
            StiffnessInfluence = stiffnessInfluence;
            ThicknessInfluence = thicknessInfluence;
        }

        public override Panel Convolution(Panel panel)
        {
            //thickness convolution
            //calculate weighted average curvature (density weighted)
            //calculate average density
            for(int i = 0; i < panel.HMaxels.GetLength(0);  i++)
            {
                for(int j = 0; i < panel.HMaxels.GetLength(1); j++)
                {
                    double averageDensity = 0;
                    double weightedRadius = 0;
                    double totalWeights = 0;
                    HMaxel hmaxel = panel.HMaxels[i, j];
                    List<Tuple<double,double>> tuples = new List<Tuple<double,double>>();
                    for(int v = 0; v < hmaxel.ActiveBoards.Count; v++)
                    {
                        averageDensity += (double)hmaxel.ActiveBoards[v].Attributes["density"];
                        weightedRadius += (hmaxel.ActiveBoards[v].Radius * (double)hmaxel.ActiveBoards[v].Attributes["density"]);
                        totalWeights += (double)hmaxel.ActiveBoards[v].Attributes["density"];
                    }
                    averageDensity /= hmaxel.ActiveBoards.Count;
                    weightedRadius /= totalWeights;
                    panel.HMaxels[i, j].Attributes.Add("avg density", averageDensity);
                    panel.HMaxels[i, j].Attributes.Add("weighted radius", weightedRadius);
                }
            }

            //horizontal convolution
            //apply old curvature convolution to BlendedRadius of HMaxels
        }

        

    public class MyConvolution : ConvolutionEngine
    {
        double Tangential { get; set; }
        double Longitudinal { get; set; }
        double RadiusMax { get; set; }
        double StiffnessInfluence { get; set; }
        double ThicknessInfluence { get; set; }

        public MyConvolution(double tangential, double longitudinal, double radiusMax, double stiffnessInfluence, double thicknessInfluence)
        {
            Tangential = tangential;
            Longitudinal = longitudinal;
            RadiusMax = radiusMax;
            StiffnessInfluence = stiffnessInfluence;
            ThicknessInfluence = thicknessInfluence;
        }

        public override Panel Convolution(Panel panel)
        {
            panel = SetConvolutionFactors(panel, RadiusMax, StiffnessInfluence);
            panel = CurvatureConvolution(panel, Longitudinal, Tangential);
            return ThicknessConvolution(panel, ThicknessInfluence);
        }

        public Panel SetConvolutionFactors(Panel panel, double maxRadiusInfluence, double stiffnessFactor)
        {
            //get stiffness bounds
            double maxLongStiffness = 0;
            double minLongStiffness = double.MaxValue;
            double maxHorizStiffness = 0;
            double minHorizStiffness = double.MaxValue;
            double minCurvature = double.MaxValue;

            foreach (Bilayer bilayer in panel.Bilayers)
            {
                foreach (ActiveBoard board in bilayer.Boards)
                {
                    if (board.Species.Attributes["LEmod"] > maxLongStiffness) maxLongStiffness = board.Species.Attributes["LEmod"];
                    if (board.Species.Attributes["LEmod"] < minLongStiffness) minLongStiffness = board.Species.Attributes["LEmod"];
                    if (board.Species.Attributes["REmod"] > maxHorizStiffness) maxHorizStiffness = board.Species.Attributes["REmod"];
                    if (board.Species.Attributes["REmod"] < minHorizStiffness) minHorizStiffness = board.Species.Attributes["REmod"];
                    if (board.Radius < minCurvature) minCurvature = board.Radius;
                }
            }

            foreach (Bilayer bilayer in panel.Bilayers)
            {
                //set factors
                foreach (ActiveBoard board in bilayer.Boards)
                {
                    board.LongStiffnessFactor = Panel.Remap(board.Species.Attributes["LEmod"], minLongStiffness, maxLongStiffness, 1 - stiffnessFactor, 1);
                    board.RadStiffnessFactor = Panel.Remap(board.Species.Attributes["REmod"], minHorizStiffness, maxHorizStiffness, 1 - stiffnessFactor, 1);

                    board.RadiusFactor = Panel.Remap(board.Radius, minCurvature, maxRadiusInfluence, 1, 0);
                }
            }

            return panel;

        }

        public Panel CurvatureConvolution(Panel panel, double horizontal, double vertical)
        {
            foreach (Bilayer bilayer in panel.Bilayers)
            {
                foreach (ActiveBoard board in bilayer.Boards)
                {
                    board.ConvolutionWeights = new List<Tuple<double, double>>();

                    foreach (ActiveBoard testBoard in bilayer.Boards)
                    {
                        double closestX = double.MaxValue;
                        double closestY = double.MaxValue;

                        if (Math.Abs(testBoard.RowRange[0] - board.Centroid.X) < closestX) closestX = Math.Abs(testBoard.RowRange[0] - board.Centroid.X);
                        if (Math.Abs(testBoard.RowRange[1] - board.Centroid.X) < closestX) closestX = Math.Abs(testBoard.RowRange[1] - board.Centroid.X);
                        if (testBoard.RowRange[0] <= board.Centroid.X + 10 && testBoard.RowRange[1] >= board.Centroid.X - 10) closestX = 0;
                        if (Math.Abs(testBoard.Centroid.X - board.Centroid.X) < closestX) closestX = Math.Abs(testBoard.Centroid.X - board.Centroid.X);

                        if (Math.Abs(testBoard.ColumnRange[0] - board.Centroid.Y) < closestY) closestY = Math.Abs(testBoard.ColumnRange[0] - board.Centroid.Y);
                        if (Math.Abs(testBoard.ColumnRange[1] - board.Centroid.Y) < closestY) closestY = Math.Abs(testBoard.ColumnRange[1] - board.Centroid.Y);
                        if (testBoard.ColumnRange[0] <= board.Centroid.Y + 10 && testBoard.ColumnRange[1] >= board.Centroid.Y - 10) closestY = board.Width / 2;
                        if (Math.Abs(testBoard.Centroid.Y - board.Centroid.Y) < closestY) closestY = Math.Abs(testBoard.Centroid.Y - board.Centroid.Y);

                        double LongFactor = testBoard.LongStiffnessFactor / Math.Pow(closestX + 1, horizontal);
                        double RadFactor = testBoard.RadStiffnessFactor / Math.Pow(closestY + 1, vertical);

                        board.ConvolutionWeights.Add(new Tuple<double, double>(testBoard.Radius, testBoard.RadiusFactor * (LongFactor * RadFactor)));
                    }
                }

                foreach (ActiveBoard board in bilayer.Boards)
                {

                    double weightSum = 0;
                    foreach (Tuple<double, double> valueWeight in board.ConvolutionWeights)
                    {
                        board.BlendedRadius += valueWeight.Item1 * valueWeight.Item2;
                        weightSum += valueWeight.Item2;

                    }
                    board.BlendedRadius /= weightSum;

                }
            }
            return panel;
        }

        public Panel ThicknessConvolution(Panel panel, double thicknessFactor)
        {
            double minThickness = double.MaxValue;
            double maxThickness = 0;
            foreach (Bilayer bilayer in panel.Bilayers)
            {
                double thickness = bilayer.ActiveThickness + bilayer.PassiveLayer.Height;
                if (thickness < minThickness) minThickness = thickness;
                if (thickness > maxThickness) maxThickness = thickness;
            }

            panel.FindThicknessNeighbors();

            foreach (Bilayer bilayer in panel.Bilayers)
            {
                double bilayerThicknessFactor = 1;
                if (minThickness != maxThickness) bilayerThicknessFactor = Panel.Remap(bilayer.ActiveThickness + bilayer.PassiveLayer.Height, minThickness, maxThickness, 1 - thicknessFactor, 1);

                foreach (ActiveBoard board in bilayer.Boards)
                {
                    double weights = board.RadStiffnessFactor + bilayerThicknessFactor;
                    double weightedTotal = board.BlendedRadius * weights;

                    foreach (BoardRegion region in board.Regions)
                    {
                        foreach (BoardRegion regionNeighbor in region.RegionStack)
                        {
                            double neighborThicknessFactor = 1;
                            if (minThickness != maxThickness) neighborThicknessFactor = Panel.Remap(regionNeighbor.Parent.Parent.ActiveThickness + regionNeighbor.Parent.Parent.PassiveLayer.Height, minThickness, maxThickness, 1 - thicknessFactor, 1);

                            weights += regionNeighbor.Parent.RadStiffnessFactor + neighborThicknessFactor;

                            weightedTotal += regionNeighbor.Parent.BlendedRadius * (regionNeighbor.Parent.RadStiffnessFactor + neighborThicknessFactor);
                        }

                        region.ThicknessBlendedRadius = weightedTotal / weights;
                    }
                }
            }
            return panel;
        }
    }
}