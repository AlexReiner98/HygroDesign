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
        public double Density;
        public double LExpansion;
        public double RExpansion;
        public double TExpansion;
        public double LElasticModulus;
        public double RElasticModulus;
        public double TElasticModulus;

        public Species(string name, double density, double LExp, double RExp, double TExp, double LElast, double RElast, double TElast)
        {
            Name = name;
            Density = density;
            LExpansion = LExp;
            RExpansion = RExp;
            TExpansion = TExp;
            LElasticModulus = LElast;
            RElasticModulus = RElast;
            TElasticModulus = TElast;
        }

        public Species(Species source)
        {
            Name = source.Name;
            Density = source.Density;
            LExpansion = source.LExpansion;
            RExpansion = source.RExpansion;
            TExpansion = source.TExpansion;
            LElasticModulus = source.LElasticModulus;
            RElasticModulus = source.RElasticModulus;
            TElasticModulus = source.TElasticModulus;
        }

        public Species() 
        {
            Name = null;
        }
    }
}