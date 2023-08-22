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
        public Panel(double hmaxelLength, double hmaxelWidth, int lengthCount, int widthCount)
        {
            LengthCount = lengthCount;
            WidthCount = widthCount;
            HMaxels = new HMaxel[widthCount, lengthCount];

            double totalWidth = 0;
            for(int i = 0; i < WidthCount; i++)
            {
                double totalLength = 0;
                for(int j = 0; j < LengthCount; j++)
                {
                    HMaxels[i, j] = new HMaxel(new Interval(totalLength, totalLength + hmaxelLength), new Interval(totalWidth, totalWidth + hmaxelWidth), this, i, j);
                    totalLength += hmaxelLength;
                }
                totalWidth += hmaxelWidth;
            }
        }

        public static Panel DeepCopy(Panel source)
        {
            Panel panel = new Panel(source.HMaxels[0, 0].Length, source.HMaxels[0, 0].Width, source.LengthCount, source.WidthCount);
            HMaxel[,] hmaxels = new HMaxel[source.WidthCount, source.LengthCount];
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
                    List<Bilayer> newBilayers = new List<Bilayer>();
                    foreach (Bilayer oldBilayer in hmaxels[i,j].Bilayers)
                    {
                        newBilayers.Add(bilayers[oldBilayer.ID]);
                    }
                    hmaxels[i,j].Bilayers = newBilayers;
                }
            }
            panel.HMaxels = hmaxels;
            panel.Bilayers = bilayers;
            return panel;
        }
    }
}