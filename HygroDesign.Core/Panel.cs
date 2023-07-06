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

            foreach(Bilayer bilayer in Bilayers)
            {
                bilayer.Parent = this;
            }
        }

        public static Panel DeepCopy(Panel source)
        {
            List<Bilayer> copies = new List<Bilayer>();
            foreach(Bilayer bilayer in source.Bilayers)
            {
                copies.Add(Bilayer.DeepCopy(bilayer));
            }
            return new Panel(copies);
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

        public void ThicknessConvolution()
        {
            FindThicknessNeighbors();
            foreach(Bilayer bilayer in Bilayers)
            {
                foreach(PanelBoard board in bilayer.Boards)
                {
                    double weights = board.RadStiffnessFactor;
                    double weightedTotal = board.BlendedRadius * board.RadStiffnessFactor;

                    foreach(PanelBoard thicknessNeighbor in  board.ThicknessNeighbors)
                    {
                        weights += thicknessNeighbor.RadStiffnessFactor;
                        weightedTotal += thicknessNeighbor.BlendedRadius * thicknessNeighbor.RadStiffnessFactor;
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
                    List<PanelBoard> sortedColumn = column.OrderBy(o => o.Centroid.Y).ToList();
                    output.Add(sortedColumn);
                }
                
            }
            return output;
        }
        /*
        public List<List<PanelBoard>> GetXRangeSets()
        {
            List<double> uniqueStartPoints = new List<double>();
            List<double> uniqueEndPoints = new List<double>();

            //find unique start points and endpoints
            foreach (PanelBoard panelBoard in Boards)
            {
                if (!uniqueStartPoints.Contains(panelBoard.RowRange[0])) uniqueStartPoints.Add(panelBoard.RowRange[0]);
                if (!uniqueEndPoints.Contains(panelBoard.RowRange[1])) uniqueEndPoints.Add(panelBoard.RowRange[1]);
            }

            //sort startpoints and endpoints
            List<double> sortedStartPoints = uniqueStartPoints.OrderBy(o => o).ToList();
            List<double> sortedEndPoints = uniqueEndPoints.OrderBy(o => o).ToList();

            List<List<PanelBoard>> output = new List<List<PanelBoard>>();

            //find which boards have that startpoint in thier range (startpoint <= , endpoint >)
            for (int i = 0; i < sortedStartPoints.Count; i++)
            {
                List<PanelBoard> column = new List<PanelBoard>();
                foreach (PanelBoard panelBoard in Boards)
                {
                    if (panelBoard.RowRange[0] >= sortedStartPoints[i] & panelBoard.RowRange[0] < sortedEndPoints[i] | panelBoard.RowRange[1] > sortedStartPoints[i] & panelBoard.RowRange[1] <= sortedEndPoints[i])
                    {
                        column.Add(panelBoard);
                    }
                }
                List<PanelBoard> sortedColumn = column.OrderBy(o => o.Centroid.Y).ToList();
                output.Add(sortedColumn);
            }
            return output;
        }
        */
    }
}