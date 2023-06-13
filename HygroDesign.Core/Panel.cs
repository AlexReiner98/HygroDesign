using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class Panel
    {
        public PanelBoard[][] Boards { get; set; }

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
    }
}