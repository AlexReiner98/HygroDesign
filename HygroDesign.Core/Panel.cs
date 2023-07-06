using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using static Rhino.UI.Controls.CollapsibleSectionImpl;


namespace BilayerDesign
{
    public class Panel
    {
        public PanelBoard[][] Boards { get; set; }
        public List<PanelBoard> BoardList { get; set; }

        public Plane BasePlane { get; set; }
        public Surface Surface { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }

        public int LengthCount { get; set; }
        public int WidthCount { get; set; }
        public double BoardWidth { get; set; }
        public double BoardLength { get; set; }
        public int ID { get; set; }


        public Panel(Plane basePlane, double boardWidth, double boardLength, int widthCount, int lengthCount)
        {
            BasePlane = basePlane;
            LengthCount = lengthCount;
            WidthCount = widthCount;
            BoardWidth = boardWidth;
            BoardLength = boardLength; 
            Length = boardLength * lengthCount;
            Width = boardWidth * widthCount;
            Boards = new PanelBoard[WidthCount][];

            //create surface
            Surface = Brep.CreatePlanarBreps(new Rectangle3d(BasePlane, Length, Width).ToNurbsCurve(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0].Faces[0];

            //generate boards
            GenerateBoards();
        }

        public static Panel DeepCopy(Panel source)
        {
            Plane basePlane = new Plane(source.BasePlane);
            Panel panel = new Panel(basePlane, source.BoardWidth, source.BoardLength, source.WidthCount, source.LengthCount);
            panel.ID = source.ID;

            int arrayCount = 0;
            foreach(PanelBoard[] boards in source.Boards) 
            {
                PanelBoard[] copyBoards = new PanelBoard[boards.Length];
                int boardCount = 0;
                foreach(PanelBoard board in boards)
                {
                    panel.Boards[arrayCount][boardCount] = PanelBoard.DeepCopy(board, panel);
                    boardCount++;
                }
                arrayCount++;
            }
            return panel;
        }
        

        private void GenerateBoards()
        {
            for(int i = 0; i < WidthCount; i++)
            {
                //even value rows
                if(i % 2 == 0)
                {
                    PanelBoard[] row = new PanelBoard[LengthCount];

                    for(int j = 0; j < LengthCount; j++)
                    {
                        Interval rowRange = new Interval(j* BoardLength, (j+1)* BoardLength);
                        Interval colRange = new Interval(Width - (i*BoardWidth), Width - ((i+1)* BoardWidth));
                        PanelBoard board = new PanelBoard(rowRange, colRange,this);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        row[j] = board;
                    }
                    Boards[i] = row;
                }

                //odd value rows
                else
                {
                    PanelBoard[] row = new PanelBoard[LengthCount+1];

                    for (int j = 0; j < LengthCount+1; j++)
                    {
                        Interval colRange = new Interval(Width - (i * BoardWidth), Width - ((i + 1) * BoardWidth));
                        Interval rowRange;

                        //first half board
                        if (j == 0)
                        {
                            rowRange = new Interval(0, BoardLength / 2);
                        }
                        else if (j == LengthCount)
                        {
                            rowRange = new Interval(j * BoardLength - BoardLength/2, (j * BoardLength));
                        }
                        else 
                        {
                        rowRange = new Interval( (j * BoardLength)- (BoardLength / 2),((j+1) * BoardLength) - (BoardLength / 2));
                        }

                        PanelBoard board = new PanelBoard( rowRange, colRange,this);
                        board.RowNumber = i;
                        board.ColumnNumber = j;
                        row[j] = board;
                    }
                    Boards[i] = row;
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

            BoardList = new List<PanelBoard>();

            foreach (PanelBoard[] boards in Boards)
            {
                foreach (PanelBoard board in boards)
                {
                    if (board.Material.LElasticModulus > maxLongStiffness) maxLongStiffness = board.Material.LElasticModulus;
                    if (board.Material.LElasticModulus < minLongStiffness) minLongStiffness = board.Material.LElasticModulus;
                    if (board.Material.RElasticModulus > maxHorizStiffness) maxHorizStiffness = board.Material.RElasticModulus;
                    if (board.Material.RElasticModulus < minHorizStiffness) minHorizStiffness = board.Material.RElasticModulus;
                    if (board.Radius < minCurvature) minCurvature = board.Radius;
                    BoardList.Add(board);
                }
            }

            //set factors
            foreach (PanelBoard[] boards in Boards)
            {
                foreach (PanelBoard board in boards)
                {
                    board.LongStiffnessFactor = Remap(board.Material.LElasticModulus, minLongStiffness, maxLongStiffness, 1 - stiffnessFactor,1);
                    board.RadStiffnessFactor = Remap(board.Material.RElasticModulus, minHorizStiffness, maxHorizStiffness, 1 - stiffnessFactor,1 );

                    board.RadiusFactor = Remap(board.Radius, minCurvature, maxRadiusInfluence, 1, 0);
                }
            }
        }

        public void CurvatureConvolution(double horizontal, double vertical)
        {
            foreach (PanelBoard board in BoardList)
            {
                board.ConvolutionWeights = new List<Tuple<double, double>>();
                board.WeightsNested = new List<List<double>>();
                var innerWeights = new List<double>();

                foreach (PanelBoard[] testBoards in Boards)
                {
                    innerWeights = new List<double>();
                    foreach (PanelBoard testBoard in testBoards)
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

                        innerWeights.Add(testBoard.RadiusFactor * (LongFactor * RadFactor));
                    }
                    board.WeightsNested.Add(innerWeights);
                      

                }
            }

            foreach (PanelBoard[] boards in Boards)
            {
                foreach (PanelBoard board in boards)
                {
                    double weightSum = 0;
                    foreach(Tuple<double,double> valueWeight in board.ConvolutionWeights)
                    {
                        board.BlendedRadius += valueWeight.Item1 * valueWeight.Item2;
                        weightSum += valueWeight.Item2;
                    }
                    board.BlendedRadius /= weightSum;
                }
            }
        }

        public List<List<double>> GetNeighborWeights(List<int> indexes)
        {
            PanelBoard currentBoard = Boards[indexes[0]][indexes[1]];
            return currentBoard.WeightsNested;
        }

        public List<List<PanelBoard>> GetXRangeSets()
        {
            List<double> uniqueStartPoints = new List<double>();
            List<double> uniqueEndPoints = new List<double>();

            //find unique start points and endpoints
            foreach (PanelBoard panelBoard in BoardList)
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
                foreach (PanelBoard panelBoard in BoardList)
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

        public static double Remap(double val, double from1, double to1, double from2, double to2)
        {
            return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}