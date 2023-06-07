using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class PanelBoard : BoardBase
    {
        Panel Parent;
        Interval RowRange;

        List<PanelBoard> BoardsAbove = new List<PanelBoard>();
        List<PanelBoard> BoardsBelow = new List<PanelBoard>();
        PanelBoard BoardLeft;
        PanelBoard BoardRight;
    }
}