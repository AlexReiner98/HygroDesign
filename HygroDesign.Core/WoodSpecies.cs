using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using static System.Net.Mime.MediaTypeNames;


namespace BilayerDesign
{
    public class Species
    {
        public string Name;
        public Dictionary<string, double> Attributes;

        public Species(string name, Dictionary<string, double> attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        public Species(Species source)
        {
            Name = source.Name;
            Attributes = new Dictionary<string,double>(source.Attributes);
        }
    }
}