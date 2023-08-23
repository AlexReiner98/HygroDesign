﻿using System;
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
        private double TotalHeight { get; set; }
        
        public Bilayer(Panel panel, int boardLength, List<int> boardOffsets, Species passiveSpecies, double activeThickness, double passiveThickness)
        {
            Panel = panel;
            BoardLength = boardLength;
            BoardOffsets = boardOffsets;

            PassiveLayer = new PassiveLayer(this, passiveSpecies, passiveThickness);
            ActiveLayer = new ActiveLayer(this, activeThickness);

            Thickness = activeThickness + passiveThickness;

            TotalHeight = Thickness;
            foreach(Bilayer bilayer in Panel.Bilayers)
            {
                TotalHeight += bilayer.Thickness;
            }

            GenerateBoards();
        }

        public static Bilayer DeepCopy(Bilayer source, Panel panel)
        {
            Bilayer bilayer = new Bilayer(panel, source.BoardLength, source.BoardOffsets, source.PassiveLayer.Species, source.ActiveLayer.Thickness, source.PassiveLayer.Thickness);
            bilayer.ActiveLayer = ActiveLayer.DeepCopy(source.ActiveLayer, bilayer);
            bilayer.ID = source.ID;
            return bilayer;
        }

        private void GenerateBoards()
        {
            for(int i = 0; i < Panel.Width; i++)
            {
                //pick offset based on which row we are on
                int thisOffset = 0;
                for(int v = 0; v < BoardOffsets.Count; v++)
                {
                    if(i % v == 0)
                    {
                        thisOffset = BoardOffsets[v];
                        break;
                    }
                }

                //create boards with that offset pattern
                List<HMaxel> boardMaxels = new List<HMaxel>();
                for(int j = 0; j < Panel.Length; j++)
                {
                    if (Panel.HMaxels[i, j].Height < TotalHeight)
                    {
                        if (boardMaxels.Count != 0)
                        {
                            ActiveLayer.Boards.Add(new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count));
                            boardMaxels.Clear();
                        }
                        continue;
                    }

                    if (j < thisOffset)
                    {
                        boardMaxels.Add(Panel.HMaxels[i, j]);
                        Panel.HMaxels[i, j].PassiveLayers.Add(PassiveLayer);
                    }
                    else if (j == thisOffset)
                    {
                        ActiveLayer.Boards.Add(new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count));
                        boardMaxels.Clear();
                        boardMaxels.Add(Panel.HMaxels[i, j]);
                        Panel.HMaxels[i, j].PassiveLayers.Add(PassiveLayer);

                    }
                    else if (j % BoardLength == 0)
                    {
                        ActiveLayer.Boards.Add(new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count));
                        boardMaxels.Clear();
                        boardMaxels.Add(Panel.HMaxels[i, j]);
                        Panel.HMaxels[i, j].PassiveLayers.Add(PassiveLayer);

                    }
                    else if (j == Panel.Length - 1)
                    {
                        ActiveLayer.Boards.Add(new ActiveBoard(boardMaxels, ActiveLayer, ActiveLayer.Boards.Count));
                    }
                    else
                    {
                        boardMaxels.Add(Panel.HMaxels[i, j]);
                        Panel.HMaxels[i, j].PassiveLayers.Add(PassiveLayer);
                    }
                }
            }
        }
    }
}