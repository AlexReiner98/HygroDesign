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
        public Panel Panel { get; set; }
        public ActiveLayer ActiveLayer { get; set; }
        public PassiveLayer PassiveLayer { get; set; }
        private int BoardLength { get; set; }
        private List<int> BoardOffsets { get; set; }
        public double TotalHeight { get; set; }
        
        public Bilayer(Panel panel, int boardLength, List<int> boardOffsets, Species passiveSpecies, double activeThickness, double passiveThickness)
        {
            Panel = panel;
            BoardLength = boardLength;
            BoardOffsets = boardOffsets;

            PassiveLayer = new PassiveLayer(this, passiveSpecies, passiveThickness);
            ActiveLayer = new ActiveLayer(this, activeThickness);

            Thickness = activeThickness + passiveThickness;
        }

        public static Bilayer DeepCopy(Bilayer source, Panel panel)
        {
            Bilayer bilayer = new Bilayer(panel, source.BoardLength, source.BoardOffsets, source.PassiveLayer.Species, source.ActiveLayer.Thickness, source.PassiveLayer.Thickness);
            bilayer.ActiveLayer = ActiveLayer.DeepCopy(source.ActiveLayer, bilayer);
            bilayer.ID = source.ID;
            bilayer.TotalHeight = source.TotalHeight;
            return bilayer;
        }

        public void GenerateBoards()
        {
            for(int i = 0; i < Panel.WidthCount; i++)
            {
                //pick offset based on which row we are on
                int thisOffset = 0;
                thisOffset = BoardOffsets[i % BoardOffsets.Count];

                //create boards with that offset pattern
                List<HMaxel> boardMaxels = new List<HMaxel>();
                int lengthCount = 0;
                for(int j = 0; j < Panel.LengthCount; j++)
                {
                    if (Panel.HMaxels[i, j].Height < TotalHeight)
                    {
                        if (boardMaxels.Count != 0)
                        {
                            ActiveBoard board = new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count);

                            ActiveLayer.Boards.Add(board);
                            foreach (HMaxel maxel in boardMaxels) maxel.ActiveBoards.Add(board);

                            boardMaxels = new List<HMaxel>();
                        }
                    }

                    if ((j == thisOffset || lengthCount == BoardLength) && boardMaxels.Count != 0 && Panel.HMaxels[i, j].Height >= TotalHeight)
                    {
                        ActiveBoard board = new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count);

                        ActiveLayer.Boards.Add(board);
                        foreach (HMaxel maxel in boardMaxels) maxel.ActiveBoards.Add(board);

                        boardMaxels = new List<HMaxel>();
                    }

                    if (j == thisOffset || lengthCount == BoardLength) lengthCount = 0;

                    if(Panel.HMaxels[i, j].Height >= TotalHeight)
                    {
                        boardMaxels.Add(Panel.HMaxels[i, j]);
                        Panel.HMaxels[i, j].PassiveLayers.Add(PassiveLayer);
                    }

                    if (j == Panel.LengthCount - 1 && Panel.HMaxels[i, j].Height >= TotalHeight)
                    {
                        ActiveBoard board = new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count);
                        ActiveLayer.Boards.Add(board);
                        foreach (HMaxel maxel in boardMaxels) maxel.ActiveBoards.Add(board);
                    }
                    lengthCount++;
                }
            }
        }
    }
}