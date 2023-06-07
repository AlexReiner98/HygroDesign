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
        public Surface Surface;


        public Panel(Surface surface, double BoardWidth, double BoardLength)
        {
            Surface Surface = surface;

            BoundingBox bbox = Surface.GetBoundingBox(true);
            double length = bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(1, 0, 0));
            double width = bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(0, 1, 0));

            int boardCountLength = (int)Math.Ceiling(length / BoardLength);
            int boardCountWidth = (int)Math.Ceiling(width / BoardWidth);

            for(int i = 0; i < boardCountWidth; i++)
            {
                for(int j = 0; j < boardCountLength; j++)
                {

                }
            }    
        }
    }
}