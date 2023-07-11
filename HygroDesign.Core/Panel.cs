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

        public void FindThicknessNeighbors()
        {
            
            foreach(Bilayer bilayer in Bilayers)
            {
                foreach(PanelBoard board in bilayer.Boards)
                {
                    board.ThicknessNeighbors = new List<PanelBoard>();
                    foreach(Bilayer otherBilayer in Bilayers)
                    {
                        if (otherBilayer == bilayer) continue;
                        foreach(PanelBoard otherBoard in otherBilayer.Boards)
                        {
                            //if ((otherBoard.ColumnRange == board.ColumnRange && otherBoard.RowRange == board.RowRange) || (otherBoard.ColumnRange == board.ColumnRange && otherBoard.RowRange[0] > board.RowRange[0]+1 && otherBoard.RowRange[0] < board.RowRange[1]-1) || (otherBoard.ColumnRange == board.ColumnRange && otherBoard.RowRange[1] > board.RowRange[0]+1 && otherBoard.RowRange[1] < board.RowRange[1]-1))
                            if(otherBoard.ColumnRange == board.ColumnRange && ((otherBoard.RowRange == board.RowRange)||(otherBoard.RowRange[0] < board.RowRange[1] && otherBoard.RowRange[1] > board.RowRange[0])))
                            {
                                board.ThicknessNeighbors.Add(otherBoard);
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
                double thickness = bilayer.ActiveThickness + bilayer.PassiveThickness;
                if (thickness < minThickness) minThickness = thickness;
                if (thickness > maxThickness) maxThickness = thickness;
            }

            FindThicknessNeighbors();

            foreach(Bilayer bilayer in Bilayers)
            {
                double bilayerThicknessFactor = 1;
                if (minThickness != maxThickness) bilayerThicknessFactor = Remap(bilayer.ActiveThickness + bilayer.PassiveThickness, minThickness, maxThickness, 1 - thicknessFactor, 1);
                
                foreach(PanelBoard board in bilayer.Boards)
                {
                    double weights = board.RadStiffnessFactor + bilayerThicknessFactor;
                    double weightedTotal = board.BlendedRadius * weights;

                    foreach(PanelBoard thicknessNeighbor in  board.ThicknessNeighbors)
                    {
                        double neighborThicknessFactor = 1;
                        if(minThickness != maxThickness) neighborThicknessFactor = Remap(thicknessNeighbor.Parent.ActiveThickness + thicknessNeighbor.Parent.PassiveThickness, minThickness, maxThickness, 1 - thicknessFactor, 1);
                        
                        weights += thicknessNeighbor.RadStiffnessFactor + neighborThicknessFactor;
                        weightedTotal += thicknessNeighbor.BlendedRadius * (thicknessNeighbor.RadStiffnessFactor + neighborThicknessFactor);
                    }
                    board.ThicknessBlendedRadius = weightedTotal / weights;
                }
            }
        }

        public List<List<PanelBoard>> GetXRangeSets()
        {
            List<double> uniqueStartPoints = new List<double>();
            List<double> uniqueEndPoints = new List<double>();

            //find unique start points and endpoints
            foreach (Bilayer bilayer in Bilayers)
            {
                foreach(PanelBoard board in bilayer.Boards)
                {
                    if (!uniqueStartPoints.Contains(board.RowRange[0])) uniqueStartPoints.Add(board.RowRange[0]);
                    if (!uniqueEndPoints.Contains(board.RowRange[1])) uniqueEndPoints.Add(board.RowRange[1]);
                }
            }

            //sort startpoints and endpoints
            List<double> sortedStartPoints = uniqueStartPoints.OrderBy(o => o).ToList();
            List<double> sortedEndPoints = uniqueEndPoints.OrderBy(o => o).ToList();

            List<List<PanelBoard>> output = new List<List<PanelBoard>>();

            //find which boards have that startpoint in thier range (startpoint <= , endpoint >)
            for (int i = 0; i < sortedStartPoints.Count; i++)
            {
                List<PanelBoard> column = new List<PanelBoard>();
                foreach(Bilayer bilayer in Bilayers)
                {
                    foreach (PanelBoard board in bilayer.Boards)
                    {
                        if ((board.RowRange[0] >= sortedStartPoints[i] & board.RowRange[0] < sortedEndPoints[i] | board.RowRange[1] > sortedStartPoints[i] & board.RowRange[1] <= sortedEndPoints[i]))
                        {
                            bool alreadyIncluded = false;
                            foreach(PanelBoard thicknessNeighbor in board.ThicknessNeighbors)
                            {
                                if(column.Contains(thicknessNeighbor))alreadyIncluded = true;
                            }
                            if(!alreadyIncluded) column.Add(board);
                        }
                    }
                }
                List<PanelBoard> sortedColumn = column.OrderBy(o => o.Centroid.Y).ToList();
                output.Add(sortedColumn);

            }
            return output;
        }

        public void ApplyThicknessGradient(List<double> thicknesses)
        {

            //apply thicknesses to boards thickness paramter
            for(int i = 0; i < thicknesses.Count; i++)
            {
                foreach(Bilayer bilayer in Bilayers)
                {
                    bilayer.Boards[i].ThicknessParameter = thicknesses[i];
                }
            }

            //remap bilayer thickness values
            List<double> bilayerThicknesses = new List<double>();
            for (int i = 0; i < Bilayers.Count; i++)
            {
                bilayerThicknesses.Add(Remap(i, 0, Bilayers.Count - 1, 0, 1));
            }
            
            List<PanelBoard> boardsForRemoval = new List<PanelBoard>();
            List<Bilayer> bilayersForRemoval = new List<Bilayer>();
            List<double> largestSmallerThicknesses = new List<double>();

            for (int i = 0; i < Bilayers.Count; i++)
            {
                //find the closest difference in thickness that is below the threshold
                double largestSmallerThickness = 0;
                double largestThickness = 0;
                for (int j = 0; j < Bilayers[i].Boards.Count; j++)
                {
                    if (Bilayers[i].Boards[j].ThicknessParameter > largestThickness) largestThickness = Bilayers[i].Boards[j].ThicknessParameter;
                    if (Bilayers[i].Boards[j].ThicknessParameter < bilayerThicknesses[i] && Bilayers[i].Boards[j].ThicknessParameter > largestSmallerThickness)
                    {
                        largestSmallerThickness = Bilayers[i].Boards[j].ThicknessParameter;
                    }
                }
                if (largestThickness < bilayerThicknesses[i])
                {
                    bilayersForRemoval.Add(Bilayers[i]);
                }
                else largestSmallerThicknesses.Add(largestSmallerThickness);
            }

            foreach(Bilayer bilayer in  bilayersForRemoval)
            {
                Bilayers.Remove(bilayer);
            }

            for(int i = 0; i < Bilayers.Count; i++)
            { 
                //find average board length
                double averageLength = 0;
                foreach(PanelBoard board in Bilayers[i].Boards)
                {
                    averageLength += board.Length;
                }
                averageLength /= Bilayers[i].Boards.Count;

                //shorten transition boards by half but don't remove them
                for (int j = 0; j < Bilayers[i].Boards.Count; j++)
                {
                    if (Math.Abs(largestSmallerThicknesses[i] - Bilayers[i].Boards[j].ThicknessParameter) <= RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    {
                        PanelBoard currentBoard = Bilayers[i].Boards[j];
                        List<PanelBoard> neighbors = Bilayers[i].GetNeighbors(j);

                        if (Bilayers[i].Boards[j].Length < averageLength) continue;

                        double lowerX = 0;
                        double higherX = 0;
                        foreach (PanelBoard neighbor in neighbors)
                        {
                            if (neighbor.Centroid.X < currentBoard.Centroid.X)
                            {
                                lowerX += neighbor.ThicknessParameter;
                            }
                            else if (neighbor.Centroid.X > currentBoard.Centroid.X)
                            {
                                higherX += neighbor.ThicknessParameter;
                            }
                        }

                        double start = Bilayers[i].Boards[j].RowRange[0];
                        double end = Bilayers[i].Boards[j].RowRange[1];

                        if (lowerX >= higherX)
                        {
                            //shorten towards lower x

                            Bilayers[i].Boards[j].RowRange = new Interval(start, start + ((end - start) / 2));
                        }
                        else
                        {
                            //shorten towards higher x
                            Bilayers[i].Boards[j].RowRange = new Interval(start + ((end - start) / 2), end);
                        }
                        Bilayers[i].Boards[j].EvaluateBoard();
                    }
                    else if (Bilayers[i].Boards[j].ThicknessParameter < bilayerThicknesses[i])
                    {
                        boardsForRemoval.Add(Bilayers[i].Boards[j]);
                    }
                }
                foreach (PanelBoard board in boardsForRemoval)
                {
                    Bilayers[i].Boards.Remove(board);
                }
                Bilayers[i].UpdatePassiveLayer();
            }
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }
}