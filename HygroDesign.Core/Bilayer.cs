using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace BilayerDesign
{
    public class Bilayer: WoodAssembly
    {
        public List<ActiveBoard> Boards { get; set; }
        public Plane BasePlane { get; set; }
        public Surface InitialSurface { get; set; }
        public int LengthCount { get; set; }
        public int WidthCount { get; set; }
        public double BoardWidth { get; set; }
        public double BoardLength { get; set; }
        public double ActiveThickness { get; set; }
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
            List<ActiveBoard> boards = source.Boards;

            Bilayer bilayer = new Bilayer(basePlane, source.BoardWidth, source.BoardLength, source.WidthCount, source.LengthCount, source.ActiveThickness, source.PassiveLayer.Thickness, source.PassiveLayer.Species, source.BoardRegionCount);
            bilayer.ID = source.ID;
            bilayer.Boards.Clear();

            for(int i = 0; i < boards.Count; i++)
            {
                bilayer.Boards.Add(ActiveBoard.DeepCopy(boards[i], bilayer));
            }

            bilayer.PassiveLayer = PassiveLayer.DeepCopy(source.PassiveLayer, bilayer);

            return bilayer;
        }
        

        private void GenerateBoards()
        {
            Boards = new List<ActiveBoard>();
            for (int i = 0; i < WidthCount; i++)
            {
                //even value rows
                if(i % 2 == 0)
                {
                    ActiveBoard[] row = new ActiveBoard[LengthCount];

                    for(int j = 0; j < LengthCount; j++)
                    {
                        Interval rowRange = new Interval(j* BoardLength, (j+1)* BoardLength);
                        Interval colRange = new Interval(Width - ((i+1)* BoardWidth), Width - (i * BoardWidth));
                        ActiveBoard board = new ActiveBoard(rowRange, colRange,this,BoardRegionCount);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        Boards.Add(board);
                    }
                }

                //odd value rows
                else
                {
                    ActiveBoard[] row = new ActiveBoard[LengthCount+1];
                    
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

                        ActiveBoard board = new ActiveBoard( rowRange, colRange,this, regionCount);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        Boards.Add(board);
                    }
                }
            }
        }

        public List<ActiveBoard> GetNeighbors(int index)
        {
            List<ActiveBoard> neighbors = new List<ActiveBoard>();

            ActiveBoard currentBoard = Boards[index];

            foreach(ActiveBoard testBoard in Boards)
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

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}