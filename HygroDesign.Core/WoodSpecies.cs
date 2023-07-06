using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using static System.Net.Mime.MediaTypeNames;


namespace BilayerDesign
{
    public class Material
    {
        public string Name; 
        public double LExpansion;
        public double RExpansion;
        public double TExpansion;
        public double LElasticModulus;
        public double RElasticModulus;
        public double TElasticModulus;

        public Material(string name, double LExp, double RExp, double TExp, double LElast, double RElast, double TElast)
        {
            Name = name;
            LExpansion = LExp;
            RExpansion = RExp;
            TExpansion = TExp;
            LElasticModulus = LElast;
            RElasticModulus = RElast;
            TElasticModulus = TElast;
        }

        public Material(Material source)
        {
            Name = source.Name;
            LExpansion = source.LExpansion;
            RExpansion = source.RExpansion;
            TExpansion = source.TExpansion;
            LElasticModulus = source.LElasticModulus;
            RElasticModulus = source.RElasticModulus;
            TElasticModulus = source.TElasticModulus;
        }

        public Material() 
        {
            Name = null;
        }
    }
}