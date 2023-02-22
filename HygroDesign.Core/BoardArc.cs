using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using System.Linq;


namespace HygroDesign.Core
{
    public class BoardArc
    {
        /// <summary>
        /// A single arc curve within a cross section.
        /// </summary>
        public Arc ArcCurve;

        /// <summary>
        /// A the radius of the arc.
        /// </summary>
        public double Radius;

        /// <summary>
        /// The arc's centroid point.
        /// </summary>
        public Point3d Centroid;

        /// <summary>
        /// The closest NurbsCurve control point id to this arc.
        /// </summary>
        public int ControlPointID;


        /// <summary>
        /// The input arc to create a BoardArc object.
        /// </summary>
        /// <param name="arc"></param>
        public BoardArc(Arc arc)
        {
            ArcCurve = arc;
            Radius = arc.Radius;
            Centroid = arc.Center;
        }
    }
}