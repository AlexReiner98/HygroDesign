using System;
using System.Collections.Generic;
using System.Linq;
using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace BilayerDesign
{

    public class Panel : WoodAssembly
    {
        public HMaxel[,] HMaxels { get; set; }
        public List<Bilayer> Bilayers { get; set; }
        public int LengthCount { get; set; }
        public int WidthCount { get; set; }
        public Brep Brep { get; set; }
        public Panel(double hmaxelLength, double hmaxelWidth, int lengthCount, int widthCount)
        {
            LengthCount = lengthCount;
            WidthCount = widthCount;
            HMaxels = new HMaxel[widthCount, lengthCount];
            Bilayers = new List<Bilayer>();

            double totalWidth = 0;
            for(int i = 0; i < WidthCount; i++)
            {
                double totalLength = 0;
                for(int j = 0; j < LengthCount; j++)
                {
                    HMaxels[i, j] = new HMaxel(new Interval(totalLength, totalLength + hmaxelLength), new Interval(totalWidth, totalWidth + hmaxelWidth), this, i, j);
                    totalLength += hmaxelLength;
                    if (totalLength > Length) Length = totalLength;
                }
                totalWidth += hmaxelWidth;
                if(totalWidth > Width) Width = totalWidth;
            }
        }

        public static Panel DeepCopy(Panel source)
        {
            Panel panel = new Panel(source.HMaxels[0, 0].Length, source.HMaxels[0, 0].Width, source.LengthCount, source.WidthCount);
            HMaxel[,] hmaxels = new HMaxel[source.WidthCount, source.LengthCount];
            panel.Brep = source.Brep;
            for(int i = 0; i < source.WidthCount; i++)
            {
                for(int j = 0; j < source.LengthCount; j++)
                {
                    hmaxels[i, j] = HMaxel.DeepCopy(source.HMaxels[i, j], panel);
                }
            }
            List<Bilayer> bilayers = new List<Bilayer>();
            foreach(Bilayer bilayer in source.Bilayers)
            {
                bilayers.Add(Bilayer.DeepCopy(bilayer, panel));
            }
            for (int i = 0; i < source.WidthCount; i++)
            {
                for (int j = 0; j < source.LengthCount; j++)
                {
                    List<PassiveLayer> newPassiveLayers = new List<PassiveLayer>();
                    List<ActiveBoard> newActiveBoards = new List<ActiveBoard>();

                    foreach (PassiveLayer oldPassiveLayer in source.HMaxels[i,j].PassiveLayers)
                    {
                        newPassiveLayers.Add(bilayers[oldPassiveLayer.ID].PassiveLayer);
                    }
                    foreach(ActiveBoard oldActiveBoard in source.HMaxels[i,j].ActiveBoards)
                    {
                        newActiveBoards.Add(bilayers[oldActiveBoard.ActiveLayer.Bilayer.ID].ActiveLayer.Boards[oldActiveBoard.ID]);
                    }

                    hmaxels[i,j].PassiveLayers = newPassiveLayers;
                    hmaxels[i,j].ActiveBoards = newActiveBoards;
                }
            }
            panel.HMaxels = hmaxels;
            panel.Bilayers = bilayers;

            for(int i = 0; i < panel.Bilayers.Count; i++)
            {
                for(int j = 0; j < panel.Bilayers[i].ActiveLayer.Boards.Count; j++)
                {
                    List<HMaxel> newhmaxels = new List<HMaxel>();
                    for(int v = 0; v < panel.Bilayers[i].ActiveLayer.Boards[j].HMaxels.Count; v++)
                    {
                        newhmaxels.Add(panel.HMaxels[panel.Bilayers[i].ActiveLayer.Boards[j].HMaxels[v].I, panel.Bilayers[i].ActiveLayer.Boards[j].HMaxels[v].J]);
                    }
                    panel.Bilayers[i].ActiveLayer.Boards[j].HMaxels = newhmaxels;
                }
            }
            return panel;
        }

        public void ComputeBoards()
        {
            double totalHeight = 0;
            for(int i = 0; i < Bilayers.Count; i++)
            {
                totalHeight += Bilayers[i].Thickness;
                Bilayers[i].TotalHeight = totalHeight;
                Bilayers[i].GenerateBoards();
            }
        }

        public void CalculateCenterOfGravity()
        {
            double xCoord = 0;
            double yCoord = 0;
            double zCoord = 0;

            double totalMass = 0;

            foreach (Bilayer bilayer in Bilayers)
            {

                foreach (ActiveBoard board in bilayer.ActiveLayer.Boards)
                {
                    //add board values
                    Point3d boardCentroid = board.ShapedCentroid;
                    RhinoApp.Write(boardCentroid.ToString());
                    if (boardCentroid.X == 0 && boardCentroid.Y == 0 && boardCentroid.Z == 0) continue;
                    double mass = board.Mass + board.PassiveMass;

                    xCoord += boardCentroid.X * mass;
                    yCoord += boardCentroid.Y * mass;
                    zCoord += boardCentroid.Z * mass;
                    totalMass += mass;
                }
            }

            //divide coords by total mass
            xCoord /= totalMass;
            yCoord /= totalMass;
            zCoord /= totalMass;

            //return point
            Centroid = new Point3d(xCoord, yCoord, zCoord);
        }

        public void GenerateShapedSurfaces(Brep brep)
        {
            if (brep.Faces.Count != 1) throw new Exception("Only single face breps are allowed.");

            Brep = brep;

            //trim hmaxels
            for(int i = 0; i < HMaxels.GetLength(0); i++)
            {
                for (int j = 0; j < HMaxels.GetLength(1); j++)
                {
                    HMaxel hmaxel = HMaxels[i, j];
                    List<Point3d> points = new List<Point3d>()
                    {
                        Brep.Faces[0].PointAt(hmaxel.RowRange[0], hmaxel.ColumnRange[0]),
                        Brep.Faces[0].PointAt(hmaxel.RowRange[1], hmaxel.ColumnRange[0]),
                        Brep.Faces[0].PointAt(hmaxel.RowRange[0], hmaxel.ColumnRange[1]),
                        Brep.Faces[0].PointAt(hmaxel.RowRange[1], hmaxel.ColumnRange[1])
                    };

                    List<int> notOnID = new List<int>();
                    for(int v = 0; v < points.Count; v++)
                    {
                        if (brep.ClosestPoint(points[v]).DistanceTo(points[v]) > 10) notOnID.Add(v);
                    }

                    if(notOnID.Count == 0) hmaxel.ShapedHMaxel = Brep.Faces[0].Trim(hmaxel.RowRange, hmaxel.ColumnRange).ToBrep();
                    else
                    {
                        Brep hmaxelBrep = Brep.Faces[0].Trim(hmaxel.RowRange, hmaxel.ColumnRange).ToBrep();
                        var results = hmaxelBrep.Split(brep.Edges, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                        for(int v = 0; v < results.Length; v++)
                        {
                            bool keepFragment = true;
                            Brep fragment = results[v];
                            for (int x = 0; x < notOnID.Count; x++) 
                            {
                                Point3d point = points[notOnID[x]];
                                
                                if (fragment.ClosestPoint(point).DistanceTo(point) < 10) keepFragment = false;
                            }
                            if (keepFragment) HMaxels[i, j].ShapedHMaxel = fragment;
                        }
                    }
                }
            }

            //trim boards
            foreach(Bilayer bilayer in Bilayers)
            {
                foreach(ActiveBoard board in bilayer.ActiveLayer.Boards)
                {
                    List<Point3d> points = new List<Point3d>()
                    {
                        Brep.Faces[0].PointAt(board.RowRange[0], board.ColumnRange[0]),
                        Brep.Faces[0].PointAt(board.RowRange[1], board.ColumnRange[0]),
                        Brep.Faces[0].PointAt(board.RowRange[0], board.ColumnRange[1]),
                        Brep.Faces[0].PointAt(board.RowRange[1], board.ColumnRange[1])
                    };

                    List<int> notOnID = new List<int>();
                    for (int v = 0; v < points.Count; v++)
                    {
                        if (brep.ClosestPoint(points[v]).DistanceTo(points[v]) > 10) notOnID.Add(v);
                    }

                    if (notOnID.Count == 0) board.ShapedBoard = Brep.Faces[0].Trim(board.RowRange, board.ColumnRange).ToBrep();
                    else
                    {
                        Brep hmaxelBrep = Brep.Faces[0].Trim(board.RowRange, board.ColumnRange).ToBrep();
                        var results = hmaxelBrep.Split(brep.Edges, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                        for (int v = 0; v < results.Length; v++)
                        {
                            bool keepFragment = true;
                            Brep fragment = results[v];
                            for (int x = 0; x < notOnID.Count; x++)
                            {
                                Point3d point = points[notOnID[x]];

                                if (fragment.ClosestPoint(point).DistanceTo(point) < 10) keepFragment = false;
                            }
                            if (keepFragment) board.ShapedBoard = fragment;
                        }
                    }
                }
            }
        }
    }
}