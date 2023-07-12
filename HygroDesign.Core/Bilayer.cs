using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;

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
        public double PassiveThickness { get; set; }
        public Species PassiveSpecies { get; set; }
        public int ID { get; set; }
        public int PanelNumber { get; set; }
        public Panel Parent { get; set; }
        public Interval PassiveLayerX { get; set; }
        public Interval PassiveLayerY { get; set; }


        public Bilayer(Plane basePlane, double boardWidth, double boardLength, int widthCount, int lengthCount, double activeThickness, double passiveThickness, Species passiveSpecies)
        {
            BasePlane = basePlane;
            LengthCount = lengthCount;
            WidthCount = widthCount;
            BoardWidth = boardWidth;
            BoardLength = boardLength; 
            Length = boardLength * lengthCount;
            Width = boardWidth * widthCount;
            ActiveThickness = activeThickness;
            PassiveThickness = passiveThickness;
            PassiveSpecies = passiveSpecies;

            //create surface
            InitialSurface = Brep.CreatePlanarBreps(new Rectangle3d(BasePlane, Length, Width).ToNurbsCurve(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].Faces[0];

            //generate boards
            GenerateBoards();
        }

        public static Bilayer DeepCopy(Bilayer source)
        {
            Plane basePlane = new Plane(source.BasePlane);
            List<PanelBoard> boards = source.Boards;

            Bilayer bilayer = new Bilayer(basePlane, source.BoardWidth, source.BoardLength, source.WidthCount, source.LengthCount, source.ActiveThickness, source.PassiveThickness, source.PassiveSpecies);
            bilayer.ID = source.ID;
            bilayer.Boards.Clear();
            bilayer.PassiveLayerX = source.PassiveLayerX;
            bilayer.PassiveLayerY = source.PassiveLayerY;

            for(int i = 0; i < boards.Count; i++)
            {
                bilayer.Boards.Add(PanelBoard.DeepCopy(boards[i], bilayer));
            }
            return bilayer;
        }
        

        private void GenerateBoards()
        {
            Boards = new List<PanelBoard>();
            for(int i = 0; i < WidthCount; i++)
            {
                //even value rows
                if(i % 2 == 0)
                {
                    PanelBoard[] row = new PanelBoard[LengthCount];

                    for(int j = 0; j < LengthCount; j++)
                    {
                        Interval rowRange = new Interval(j* BoardLength, (j+1)* BoardLength);
                        Interval colRange = new Interval(Width - ((i+1)* BoardWidth), Width - (i * BoardWidth));
                        PanelBoard board = new PanelBoard(rowRange, colRange,this,2);
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
                            regionCount = 1; ;
                        }
                        else if (j == LengthCount)
                        {
                            rowRange = new Interval(j * BoardLength - BoardLength/2, (j * BoardLength));
                            regionCount = 1;
                        }
                        else 
                        {
                        rowRange = new Interval( (j * BoardLength)- (BoardLength / 2),((j+1) * BoardLength) - (BoardLength / 2));
                            regionCount = 2;
                        }

                        PanelBoard board = new PanelBoard( rowRange, colRange,this, regionCount);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        Boards.Add(board);
                    }
                }
            }
        }

        public void SetConvolutionFactors(double maxRadiusInfluence, double stiffnessFactor)
        {
            //get stiffness bounds
            double maxLongStiffness = 0;
            double minLongStiffness = double.MaxValue;
            double maxHorizStiffness = 0;
            double minHorizStiffness = double.MaxValue;
            double minCurvature = double.MaxValue;

            foreach (PanelBoard board in Boards)
            {
                if (board.Species.LElasticModulus > maxLongStiffness) maxLongStiffness = board.Species.LElasticModulus;
                if (board.Species.LElasticModulus < minLongStiffness) minLongStiffness = board.Species.LElasticModulus;
                if (board.Species.RElasticModulus > maxHorizStiffness) maxHorizStiffness = board.Species.RElasticModulus;
                if (board.Species.RElasticModulus < minHorizStiffness) minHorizStiffness = board.Species.RElasticModulus;
                if (board.Radius < minCurvature) minCurvature = board.Radius;
                
            }

            //set factors
            foreach (PanelBoard board in Boards)
            {
                board.LongStiffnessFactor = Remap(board.Species.LElasticModulus, minLongStiffness, maxLongStiffness, 1-stiffnessFactor, 1);
                board.RadStiffnessFactor = Remap(board.Species.RElasticModulus, minHorizStiffness, maxHorizStiffness, 1-stiffnessFactor, 1);

                board.RadiusFactor = Remap(board.Radius, minCurvature, maxRadiusInfluence, 1, 0);
            }
        }

        public void CurvatureConvolution(double horizontal, double vertical)
        {
            foreach (PanelBoard board in Boards)
            {
                board.ConvolutionWeights = new List<Tuple<double, double>>();

                foreach (PanelBoard testBoard in Boards)
                {
                    double closestX = double.MaxValue;
                    double closestY = double.MaxValue;

                    if (Math.Abs(testBoard.RowRange[0] - board.Centroid.X) < closestX) closestX = Math.Abs(testBoard.RowRange[0] - board.Centroid.X);
                    if (Math.Abs(testBoard.RowRange[1] - board.Centroid.X) < closestX) closestX = Math.Abs(testBoard.RowRange[1] - board.Centroid.X);
                    if (testBoard.RowRange[0] <= board.Centroid.X + 10 && testBoard.RowRange[1] >= board.Centroid.X - 10) closestX = 0;
                    if (Math.Abs(testBoard.Centroid.X - board.Centroid.X) < closestX) closestX = Math.Abs(testBoard.Centroid.X - board.Centroid.X);

                    if (Math.Abs(testBoard.ColumnRange[0] - board.Centroid.Y) < closestY) closestY = Math.Abs(testBoard.ColumnRange[0] - board.Centroid.Y);
                    if (Math.Abs(testBoard.ColumnRange[1] - board.Centroid.Y) < closestY) closestY = Math.Abs(testBoard.ColumnRange[1] - board.Centroid.Y);
                    if (testBoard.ColumnRange[0] <= board.Centroid.Y + 10 && testBoard.ColumnRange[1] >= board.Centroid.Y - 10) closestY = board.Width/2;
                    if (Math.Abs(testBoard.Centroid.Y - board.Centroid.Y) < closestY) closestY = Math.Abs(testBoard.Centroid.Y - board.Centroid.Y);

                    double LongFactor = testBoard.LongStiffnessFactor / Math.Pow(closestX + 1, horizontal);
                    double RadFactor = testBoard.RadStiffnessFactor / Math.Pow(closestY + 1, vertical);

                    board.ConvolutionWeights.Add(new Tuple<double, double>(testBoard.Radius, testBoard.RadiusFactor * (LongFactor * RadFactor)));
                }
            }

            foreach (PanelBoard board in Boards)
            {
               
                double weightSum = 0;
                foreach(Tuple<double, double> valueWeight in board.ConvolutionWeights)
                {
                    board.BlendedRadius += valueWeight.Item1 * valueWeight.Item2;
                    weightSum += valueWeight.Item2;
                }
                board.BlendedRadius /= weightSum;
                
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

        public void UpdatePassiveLayer()
        {
            double minX = double.MaxValue;
            double maxX = 0;
            double minY = double.MaxValue;
            double maxY = 0;

            foreach(PanelBoard board in Boards)
            {
                foreach(BoardRegion region in board.BoardRegions)
                {
                    if (region.RowRange[0] < minX) minX = region.RowRange[0];
                    if (region.RowRange[1] > maxX) maxX = region.RowRange[1];
                    if (region.ColumnRange[0] < minY) minY = region.ColumnRange[0];
                    if (region.ColumnRange[1] > maxY) maxY = region.ColumnRange[1];
                }
                
            }
            minX = minX - BasePlane.Origin.X;
            maxX = maxX - BasePlane.Origin.X;
            minY = minY - BasePlane.Origin.Y;
            maxY = maxY - BasePlane.Origin.Y;

            PassiveLayerX = new Interval(minX, maxX);
            PassiveLayerY = new Interval(minY, maxY);
        }

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}