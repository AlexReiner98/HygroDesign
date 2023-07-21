using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;

namespace BilayerDesign
{
    public class Panel
    {
        public List<Bilayer> Bilayers { get; set; }
        public int ID { get; set; }
        public Surface Surface { get; set; }
        public Point3d CenterOfGravity { get; set; }

        

        public Panel(List<Bilayer> bilayers)
        {
            Bilayers = bilayers;

            for(int i = 0; i < Bilayers.Count; i++)
            {
                Bilayers[i].ID = i;
                Bilayers[i].Parent = this;
            }
        }

        public static Panel DeepCopy(Panel source)
        {
            List<Bilayer> copies = new List<Bilayer>();
            foreach(Bilayer bilayer in source.Bilayers)
            {
                copies.Add(Bilayer.DeepCopy(bilayer));
            }
            var panel = new Panel(copies);
            panel.ID = source.ID;
            return panel;
        }

        public void CalculateCenterOfGravity()
        {
            double xCoord = 0;
            double yCoord = 0;
            double zCoord = 0;

            double totalMass = 0;

            foreach(Bilayer bilayer in Bilayers)
            {
                //add passive values
                Point3d passiveCentroid = bilayer.PassiveLayer.ShapedCentroid;
                double passiveMass = bilayer.PassiveLayer.Mass;

                xCoord += passiveCentroid.X * passiveMass;
                yCoord += passiveCentroid.Y * passiveMass;
                zCoord += passiveCentroid.Z * passiveMass;
                totalMass += passiveMass;

                //if there is a locking layer, add those values

                foreach (PanelBoard board in bilayer.Boards)
                {
                    //add board values
                    Point3d boardCentroid = board.ShapedCentroid;
                    double boardMass = board.Mass;

                    xCoord += boardCentroid.X * boardMass;
                    yCoord += boardCentroid.Y * boardMass;
                    zCoord += boardCentroid.Z * boardMass;
                    totalMass += boardMass;
                }
            }

            //divide coords by total mass
            xCoord /= totalMass;
            yCoord /= totalMass;
            zCoord /= totalMass;

            //return point
            CenterOfGravity = new Point3d(xCoord,yCoord,zCoord);
        }


        public void SetConvolutionFactors(double maxRadiusInfluence, double stiffnessFactor)
        {
            //get stiffness bounds
            double maxLongStiffness = 0;
            double minLongStiffness = double.MaxValue;
            double maxHorizStiffness = 0;
            double minHorizStiffness = double.MaxValue;
            double minCurvature = double.MaxValue;

            foreach(Bilayer bilayer in Bilayers)
            {
                foreach (PanelBoard board in bilayer.Boards)
                {
                    if (board.Species.LElasticModulus > maxLongStiffness) maxLongStiffness = board.Species.LElasticModulus;
                    if (board.Species.LElasticModulus < minLongStiffness) minLongStiffness = board.Species.LElasticModulus;
                    if (board.Species.RElasticModulus > maxHorizStiffness) maxHorizStiffness = board.Species.RElasticModulus;
                    if (board.Species.RElasticModulus < minHorizStiffness) minHorizStiffness = board.Species.RElasticModulus;
                    if (board.Radius < minCurvature) minCurvature = board.Radius;
                }
            }
            
            foreach(Bilayer bilayer in Bilayers)
            {
                //set factors
                foreach (PanelBoard board in bilayer.Boards)
                {
                    board.LongStiffnessFactor = Remap(board.Species.LElasticModulus, minLongStiffness, maxLongStiffness, 1 - stiffnessFactor, 1);
                    board.RadStiffnessFactor = Remap(board.Species.RElasticModulus, minHorizStiffness, maxHorizStiffness, 1 - stiffnessFactor, 1);

                    board.RadiusFactor = Remap(board.Radius, minCurvature, maxRadiusInfluence, 1, 0);
                }
            }
            
        }

        public void CurvatureConvolution(double horizontal, double vertical)
        {
            foreach (Bilayer bilayer in Bilayers)
            {
                foreach (PanelBoard board in bilayer.Boards)
                {
                    board.ConvolutionWeights = new List<Tuple<double, double>>();

                    foreach (PanelBoard testBoard in bilayer.Boards)
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

                foreach (PanelBoard board in bilayer.Boards)
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
        }

        public void FindThicknessNeighbors()
        {
            
            foreach(Bilayer bilayer in Bilayers)
            {
                foreach(PanelBoard board in bilayer.Boards)
                {
                    foreach(BoardRegion region in board.Regions)
                    {
                        region.RegionStack = new List<BoardRegion>();
                        foreach (Bilayer otherBilayer in Bilayers)
                        {
                            if (otherBilayer == bilayer) continue;
                            foreach (PanelBoard otherBoard in otherBilayer.Boards)
                            {
                                foreach(BoardRegion otherRegion in otherBoard.Regions)
                                {
                                    
                                    if(otherRegion.ColumnRange == region.ColumnRange && otherRegion.RowRange == region.RowRange)
                                    {
                                        region.RegionStack.Add(otherRegion);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ThicknessConvolution(double thicknessFactor)
        {
            double minThickness = double.MaxValue;
            double maxThickness = 0;
            foreach(Bilayer bilayer in Bilayers)
            {
                double thickness = bilayer.ActiveThickness + bilayer.PassiveLayer.Thickness;
                if (thickness < minThickness) minThickness = thickness;
                if (thickness > maxThickness) maxThickness = thickness;
            }

            FindThicknessNeighbors();

            foreach(Bilayer bilayer in Bilayers)
            {
                double bilayerThicknessFactor = 1;
                if (minThickness != maxThickness) bilayerThicknessFactor = Remap(bilayer.ActiveThickness + bilayer.PassiveLayer.Thickness, minThickness, maxThickness, 1 - thicknessFactor, 1);
                
                foreach(PanelBoard board in bilayer.Boards)
                {
                    double weights = board.RadStiffnessFactor + bilayerThicknessFactor;
                    double weightedTotal = board.BlendedRadius * weights;

                    foreach (BoardRegion region in board.Regions)
                    {
                        foreach (BoardRegion regionNeighbor in region.RegionStack)
                        {
                            double neighborThicknessFactor = 1;
                            if (minThickness != maxThickness) neighborThicknessFactor = Remap(regionNeighbor.Parent.Parent.ActiveThickness + regionNeighbor.Parent.Parent.PassiveLayer.Thickness, minThickness, maxThickness, 1 - thicknessFactor, 1);

                            weights += regionNeighbor.Parent.RadStiffnessFactor + neighborThicknessFactor;

                            weightedTotal += regionNeighbor.Parent.BlendedRadius * (regionNeighbor.Parent.RadStiffnessFactor + neighborThicknessFactor);
                        }
                       
                        region.ThicknessBlendedRadius = weightedTotal / weights;
                    }
                }
            }
        }

        public List<List<BoardRegion>> GetXRangeSets()
        {
            List<double> uniqueStartPoints = new List<double>();

            //find unique start points and endpoints
            foreach (Bilayer bilayer in Bilayers)
            {
                foreach(PanelBoard board in bilayer.Boards)
                {
                    foreach(BoardRegion region in board.Regions)
                    {
                        double start = Math.Round(region.RowRange[0]);
                        if (!uniqueStartPoints.Contains(start)) uniqueStartPoints.Add(start);
                    }
                }
            }

            //sort startpoints and endpoints
            List<double> sortedStartPoints = uniqueStartPoints.OrderBy(o => o).ToList();

            List<List<BoardRegion>> output = new List<List<BoardRegion>>();

            //find which boards have that startpoint in thier range (startpoint <= , endpoint >)
            for (int i = 0; i < sortedStartPoints.Count; i++)
            {
                List<BoardRegion> column = new List<BoardRegion>();
                foreach(Bilayer bilayer in Bilayers)
                {
                    foreach (PanelBoard board in bilayer.Boards)
                    {
                        foreach(BoardRegion region in board.Regions)
                        {
                            double start = Math.Round(region.RowRange[0]);
                            if (start == sortedStartPoints[i])
                            {
                                bool alreadyIncluded = false;
                                foreach (BoardRegion thicknessNeighbor in region.RegionStack)
                                {
                                    if (column.Contains(thicknessNeighbor)) alreadyIncluded = true;
                                }
                                if (!alreadyIncluded) { column.Add(region); }
                            }
                        }
                    }
                }
                List<BoardRegion> sortedColumn = column.OrderBy(o => o.Centroid.Y).ToList();
                output.Add(sortedColumn);

            }
            return output;
        }

        public void ApplyThicknessGradient(List<List<double>> thicknesses)
        {

            //apply thicknesses to boards thickness paramter
            foreach(Bilayer bilayer in Bilayers)
            {
                for(int i = 0; i<  bilayer.Boards.Count; i++)
                {
                    for(int j = 0; j < bilayer.Boards[i].Regions.Count; j++)
                    {
                        bilayer.Boards[i].Regions[j].ThicknessParameter = thicknesses[i][j];
                    }
                }
            }

            //remap bilayer thickness values
            List<double> bilayerThicknesses = new List<double>();
            for (int i = 0; i < Bilayers.Count; i++)
            {
                bilayerThicknesses.Add(Remap(i, 0, Bilayers.Count - 1, 0, 1));
            }

            List<Bilayer> bilayersForRemoval = new List<Bilayer>();
            //find regions below theshold and add them to delete list
            for(int i = 0; i < Bilayers.Count; i++)
            {
                List<PanelBoard> boardsForRemoval = new List<PanelBoard>();
                foreach(PanelBoard board in Bilayers[i].Boards)
                {
                    List<BoardRegion> regionsForRemoval = new List<BoardRegion>();
                    foreach(BoardRegion region in board.Regions)
                    {
                        if(region.ThicknessParameter < bilayerThicknesses[i])
                        {
                           regionsForRemoval.Add(region);
                        }
                    }
                    foreach(BoardRegion region in regionsForRemoval)
                    {
                        board.Regions.Remove(region);
                    }
                    if(board.Regions.Count == 0)
                    {
                        boardsForRemoval.Add(board);
                    }
                }
                foreach(PanelBoard board in boardsForRemoval)
                {
                    Bilayers[i].Boards.Remove(board);
                }
                if (Bilayers[i].Boards.Count == 0)
                {
                    bilayersForRemoval.Add(Bilayers[i]);
                }
                Bilayers[i].PassiveLayer.Update();
            }
            foreach(Bilayer bilayer in bilayersForRemoval)
            {
                Bilayers.Remove(bilayer);
            }
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }
}