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

        public void SetConvolutionFactors(double maxRadiusInfluence)
        {
            //get stiffness bounds
            double maxLongStiffness = 0;
            double maxHorizStiffness = 0;
            double minCurvature = double.MaxValue;

            BoardList = new List<PanelBoard>();

            foreach (PanelBoard[] boards in Boards)
            {
                foreach (PanelBoard board in boards)
                {
                    if (board.Material.LElasticModulus > maxLongStiffness) maxLongStiffness = board.Material.LElasticModulus;
                    if (board.Material.RElasticModulus > maxHorizStiffness) maxHorizStiffness = board.Material.RElasticModulus;
                    if (board.Radius < minCurvature) minCurvature = board.Radius;
                    BoardList.Add(board);
                }
            }

            //set factors
            foreach (PanelBoard[] boards in Boards)
            {
                foreach (PanelBoard board in boards)
                {
                    board.LongStiffnessFactor = board.Material.LElasticModulus / maxLongStiffness;
                    board.RadStiffnessFactor = board.Material.RElasticModulus / maxHorizStiffness;
                    board.RadiusFactor = Remap(board.Radius, minCurvature, maxRadiusInfluence, 1, 0);
                }
            }
        }

        public void CurvatureConvolution(double horizontal, double vertical)
        {
            foreach (PanelBoard board in BoardList)
            {
                board.ConvolutionWeights = new List<Tuple<double, double>>();
                foreach (PanelBoard testBoard in BoardList)
                {
                    double LongFactor = testBoard.RadiusFactor / (Math.Pow(Math.Abs(board.Centroid.X - testBoard.Centroid.X)+1, horizontal) * testBoard.LongStiffnessFactor);
                    double RadFactor = testBoard.RadiusFactor / (Math.Pow(Math.Abs(board.Centroid.Y - testBoard.Centroid.Y)+1, vertical) * testBoard.RadStiffnessFactor);
                    board.ConvolutionWeights.Add(new Tuple<double,double>(testBoard.Radius, LongFactor * RadFactor));
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


/* int rowCount = initialPrediction.BranchCount;
    int columnCount = initialPrediction.Branch(0).Count;
    
    //create 2d arrays
    double[,] prediction = new double[columnCount, rowCount];
    double[,] stiffness = new double[columnCount, rowCount];

    //fill arrays and get bounds
    double minStiffness = double.MaxValue;
    double minRad = double.MaxValue;
    double maxRad = 0;

    for(int j = 0; j < initialPrediction.BranchCount; j++)
    {
      for(int i = 0; i < initialPrediction.Branch(j).Count; i++)
      {
        prediction[i, j] = initialPrediction.Branch(j)[i];
        stiffness[i, j] = boardStiffness.Branch(j)[i];

        if(stiffness[i, j] < minStiffness)minStiffness = stiffness[i, j];
        if(prediction[i, j] < minRad)minRad = prediction[i, j];
        if(prediction[i, j] > maxRad)maxRad = prediction[i, j];
      }
    }
    Tuple<double,double> radBounds = new Tuple<double,double>(minRad, 20000.0);

    //scale min stiffness to 1 and max stiffness proportionally
    for(int i = 0; i < columnCount; i++)
    {
      for(int j = 0; j < rowCount; j++)
      {
        stiffness[i, j] /= minStiffness;
      }
    }

    //create convolution results array
    Tuple<double,double>[,] predictionBlend = new Tuple<double,double>[columnCount, rowCount];

    //perform convolution
    for(int i = 0;  i < columnCount; i++)
    {
      for(int j = 0; j < rowCount; j++)
      {
        predictionBlend[i, j] = Convolution(i, j, prediction, radBounds, stiffness, tangentialBlend, longitudinalBlend, columnCount, rowCount);
      }
    }

    //create output tree
    DataTree<double> output = new DataTree<double>();
    for(int i = 0;  i < columnCount; i++)
    {
      for(int j = 0; j < rowCount; j++)
      {
        output.Add(predictionBlend[i, j].Item1, new GH_Path(j, i));
        output.Add(predictionBlend[i, j].Item2, new GH_Path(j, i));
      }
    }
    weightedAverages = output;

    //output weights for selected board's convolution
    selectedWeights = ConvolutionWeights(displayCol, displayRow, prediction, radBounds, stiffness, tangentialBlend, longitudinalBlend, columnCount, rowCount);

    //output display surfaces
    DataTree<Surface> surfaceOutput = new DataTree<Surface>();
    for(int i = 0; i < boards.BranchCount; i++)
    {
      for(int j = 0; j < boards.Branch(i).Count; j++)
      {
        if(i == displayRow)surfaceOutput.Add(boards.Branch(i)[j], new GH_Path(1));
        if(j == displayCol)surfaceOutput.Add(boards.Branch(i)[j], new GH_Path(0));
      }
    }
    surfaces = surfaceOutput;

public Tuple<double,double> Convolution(int colNum, int rowNum, double[,] array, Tuple<double,double> radBounds, double[,] stiffness, double tanBlend, double longBlend, int cols, int rows)
  {
    //calculate column weights
    double[] colWeights = new double[rows];
    for(int row = 0; row < rows; row++)
    {
      double radiusFactor = Math.Pow(Remap(array[colNum, row], radBounds.Item1, radBounds.Item2, 1, 0), 2);
      colWeights[row] = radiusFactor / Math.Pow((Math.Abs(row - rowNum) + 1) / stiffness[colNum, row], longBlend);
    }

    //calculate row weights
    double[] rowWeights = new double[cols];
    for(int col = 0; col < cols; col++)
    {
      double radiusFactor = Math.Pow(Remap(array[col, rowNum], radBounds.Item1, radBounds.Item2, 1, 0), 2);
      rowWeights[col] = radiusFactor / Math.Pow((Math.Abs(col - colNum) + 1) / stiffness[col, rowNum], tanBlend);
    }

    //calculate weighted averages col
    double totalValuesCol = 0;
    double totalWeightsCol = 0;
    for(int row = 0; row < rows; row++)
    {
      totalValuesCol += array[colNum, row] * colWeights[row];
      totalWeightsCol += colWeights[row];
    }
    double weightedColVal = totalValuesCol / totalWeightsCol;

    //calculate weighted averages row
    double totalValuesRow = 0;
    double totalWeightsRow = 0;
    for(int col = 0; col < cols; col++)
    {
      totalValuesRow += array[col, rowNum] * rowWeights[col];
      totalWeightsRow += rowWeights[col];
    }
    double weightedRowVal = totalValuesRow / totalWeightsRow;

    return new Tuple<double,double>(weightedColVal, weightedRowVal);
  }




  public DataTree<double> ConvolutionWeights(int colNum, int rowNum, double[,] array, Tuple<double,double> radBounds, double[,] stiffness, double tanBlend, double longBlend, int cols, int rows)
  {
    //calculate column weights
    double[] colWeights = new double[rows];
    double[] colDistance = new double[rows];
    double[] colStiffness = new double[rows];
    double[] colRadFact = new double[rows];

    for(int row = 0; row < rows; row++)
    {
      double radiusFactor = Math.Pow(Remap(array[colNum, row], radBounds.Item1, radBounds.Item2, 1, 0), 2);
      colWeights[row] = radiusFactor / Math.Pow((Math.Abs(row - rowNum) + 1) / stiffness[colNum, row], longBlend);

      //record varius factors
      colDistance[row] = Math.Abs(row - rowNum) + 1;
      colStiffness[row] = stiffness[colNum, row];
      colRadFact[row] = radiusFactor;
    }

    //calculate row weights
    double[] rowWeights = new double[cols];
    double[] rowDistance = new double[cols];
    double[] rowStiffness = new double[cols];
    double[] rowRadFact = new double[cols];

    for(int col = 0; col < cols; col++)
    {
      double radiusFactor = Math.Pow(Remap(array[col, rowNum], radBounds.Item1, radBounds.Item2, 1, 0), 2);
      rowWeights[col] = radiusFactor / Math.Pow((Math.Abs(col - colNum) + 1) / stiffness[col, rowNum], tanBlend);

      //record varius factors
      rowDistance[col] = Math.Abs(col - colNum) + 1;
      rowStiffness[col] = stiffness[col, rowNum];
      rowRadFact[col] = radiusFactor;
    }

    //output weights
    DataTree<double> output = new DataTree<double>();
    foreach(double val in colWeights)output.Add(val, new GH_Path(0, 0));
    foreach(double val in colDistance)output.Add(val, new GH_Path(0, 1));
    foreach(double val in colStiffness)output.Add(val, new GH_Path(0, 2));
    foreach(double val in colRadFact)output.Add(val, new GH_Path(0, 3));

    foreach(double val in rowWeights)output.Add(val, new GH_Path(1, 0));
    foreach(double val in rowDistance)output.Add(val, new GH_Path(1, 1));
    foreach(double val in rowStiffness)output.Add(val, new GH_Path(1, 2));
    foreach(double val in rowRadFact)output.Add(val, new GH_Path(1, 3));
    return output;
  }



  public double Remap (double val, double from1, double to1, double from2, double to2) {
    return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
  }
*/