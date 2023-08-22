using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;

namespace BilayerDesign
{
    public class ActiveLayer : WoodAssembly
    {
        public Bilayer Bilayer { get; set; }
        public List<ActiveBoard> Boards { get; set; }
        public ActiveLayer(Bilayer bilayer, double thickness)
        {
            Bilayer = bilayer;
            Thickness = thickness;
        }

        public static ActiveLayer DeepCopy(ActiveLayer source, Bilayer parent)
        {
            ActiveLayer activeLayer = new ActiveLayer(parent, source.Thickness);
            List<ActiveBoard> activeBoards = new List<ActiveBoard>();
            foreach(ActiveBoard board in source.Boards)
            {
                activeBoards.Add(ActiveBoard.DeepCopy(board, activeLayer));
            }
            activeLayer.Boards = activeBoards;
            return activeLayer;
        }
    }
}