using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class Panel
    {
        public PanelBoard[][] Boards;

        public double[][] DesiredCurvatures;
        public Material[][] DesiredMaterials;

        public Plane BasePlane;
        public Surface Surface;
        public double Length;
        public double Width;

        public int LengthCount;
        public int WidthCount;
        public double BoardWidth;
        public double BoardLength;


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
                        row[j] = new PanelBoard(this, rowRange, colRange);
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

                        row[j] = new PanelBoard(this, rowRange, colRange);
                    }
                    Boards[i] = row;
                }
            }
        }
    }
}