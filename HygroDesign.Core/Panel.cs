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
                            if (!(otherBoard.ColumnRange == board.ColumnRange && otherBoard.RowRange == board.RowRange)) continue;
                            board.ThicknessNeighbors.Add(otherBoard);
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
            List<double> remappedThicknesses = new List<double>();
            for(int i = 0; i < thicknesses.Count; i++)
            {
                remappedThicknesses.Add(Remap(thicknesses[i], 0, 1, 1, Bilayers.Count));
                
            }
            
            List<PanelBoard> forRemoval = new List<PanelBoard>();

            for(int i = 0; i < Bilayers.Count;i++)
            {
                for(int j = 0; j < Bilayers[i].Boards.Count; j++)
                {
                    if (remappedThicknesses[j] <= i)
                    {
                        forRemoval.Add(Bilayers[i].Boards[j]);
                    }
                }
                foreach (PanelBoard board in forRemoval)
                {
                    Bilayers[i].Boards.Remove(board);
                }
            }

        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }
}