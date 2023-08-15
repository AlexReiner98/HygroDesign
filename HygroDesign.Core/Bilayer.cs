﻿using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace BilayerDesign
{
    public class Bilayer
    {
        public List<PanelBoard> Boards { get; set; }
        public Plane BasePlane { get; set; }
        public Surface InitialSurface { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public int LengthCount { get; set; }
        public int WidthCount { get; set; }
        public double BoardWidth { get; set; }
        public double BoardLength { get; set; }
        public double ActiveThickness { get; set; }
        public int ID { get; set; }
        public Panel Parent { get; set; }
        public int BoardRegionCount { get; set; }
        public PassiveLayer PassiveLayer { get; set; }

        public Bilayer(Plane basePlane, double boardWidth, double boardLength, int widthCount, int lengthCount, double activeThickness, double passiveThickness, Species passiveSpecies, int boardRegionCount)
        {
            BasePlane = basePlane;
            LengthCount = lengthCount;
            WidthCount = widthCount;
            BoardWidth = boardWidth;
            BoardLength = boardLength; 
            Length = boardLength * lengthCount;
            Width = boardWidth * widthCount;
            ActiveThickness = activeThickness;
            BoardRegionCount = boardRegionCount;

            //create surface
            InitialSurface = Brep.CreatePlanarBreps(new Rectangle3d(BasePlane, Length, Width).ToNurbsCurve(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].Faces[0];

            //generate boards
            GenerateBoards();

            //generate passive layer
            PassiveLayer = new PassiveLayer(passiveThickness, passiveSpecies, this);
        }

        public static Bilayer DeepCopy(Bilayer source)
        {
            Plane basePlane = new Plane(source.BasePlane);
            List<PanelBoard> boards = source.Boards;

            Bilayer bilayer = new Bilayer(basePlane, source.BoardWidth, source.BoardLength, source.WidthCount, source.LengthCount, source.ActiveThickness, source.PassiveLayer.Thickness, source.PassiveLayer.Species, source.BoardRegionCount);
            bilayer.ID = source.ID;
            bilayer.Boards.Clear();

            for(int i = 0; i < boards.Count; i++)
            {
                bilayer.Boards.Add(PanelBoard.DeepCopy(boards[i], bilayer));
            }

            bilayer.PassiveLayer = PassiveLayer.DeepCopy(source.PassiveLayer, bilayer);

            return bilayer;
        }
        

        private void GenerateBoards()
        {
            Boards = new List<PanelBoard>();
            for (int i = 0; i < WidthCount; i++)
            {
                //even value rows
                if(i % 2 == 0)
                {
                    PanelBoard[] row = new PanelBoard[LengthCount];

                    for(int j = 0; j < LengthCount; j++)
                    {
                        Interval rowRange = new Interval(j* BoardLength, (j+1)* BoardLength);
                        Interval colRange = new Interval(Width - ((i+1)* BoardWidth), Width - (i * BoardWidth));
                        PanelBoard board = new PanelBoard(rowRange, colRange,this,BoardRegionCount);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        Boards.Add(board);
                    }
                }

                //odd value rows
                else
                {
                    PanelBoard[] row = new PanelBoard[LengthCount+1];
                    
                    for (int j = 0; j < LengthCount+1; j++)
                    {
                        Interval colRange = new Interval(Width - ((i + 1) * BoardWidth),Width - (i * BoardWidth));
                        Interval rowRange;
                        int regionCount = 0;

                        //first half board
                        if (j == 0)
                        {
                            rowRange = new Interval(0, BoardLength / 2);
                            regionCount = BoardRegionCount / 2 ;
                        }
                        else if (j == LengthCount)
                        {
                            rowRange = new Interval(j * BoardLength - BoardLength/2, (j * BoardLength));
                            regionCount = BoardRegionCount / 2;
                        }
                        else 
                        {
                        rowRange = new Interval( (j * BoardLength)- (BoardLength / 2),((j+1) * BoardLength) - (BoardLength / 2));
                            regionCount = BoardRegionCount;
                        }

                        PanelBoard board = new PanelBoard( rowRange, colRange,this, regionCount);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        Boards.Add(board);
                    }
                }
            }
        }

        public List<PanelBoard> GetNeighbors(int index)
        {
            List<PanelBoard> neighbors = new List<PanelBoard>();

            PanelBoard currentBoard = Boards[index];

            foreach(PanelBoard testBoard in Boards)
            {
                if (testBoard == currentBoard) continue;
                if ((testBoard.ColumnRange[0] <= currentBoard.ColumnRange[0] && testBoard.ColumnRange[0] <= currentBoard.ColumnRange[1]) || (testBoard.ColumnRange[1] <= currentBoard.ColumnRange[1] && testBoard.ColumnRange[1] >= currentBoard.ColumnRange[0]))
                {
                    if ((testBoard.RowRange[0] > currentBoard.RowRange[0] && testBoard.RowRange[0] < currentBoard.RowRange[1]) || (testBoard.RowRange[1] < currentBoard.RowRange[1] && testBoard.RowRange[1] > currentBoard.RowRange[0]))
                    {
                        neighbors.Add(testBoard);
                    }
                }
                
            }
            return neighbors;
        }

        public List<double> GetNeighborWeights(int index)
        {
            PanelBoard currentBoard = Boards[index];

            var output = new List<double>();
            foreach(Tuple<double,double> pair in currentBoard.ConvolutionWeights) output.Add(pair.Item2);
            
            return output;
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public override bool Equals(object other)
        {
            if (other == null || !(other is Bilayer))
                return false;
            else
                return Equals(other as Bilayer);
        }

        public bool Equals(Bilayer other)
        {
            if(other.Parent.ID == this.Parent.ID && other.ID == this.ID) return true;
            else return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Parent.ID.GetHashCode();
            hash = hash * 23 + ID.GetHashCode();
            return hash;
        }

    }
}