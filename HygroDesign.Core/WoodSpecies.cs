using System;
using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;


namespace BilayerDesign
{
    public class Material
    {
        public double LExpansion;
        public double RExpansion;
        public double TExpansion;
        public double LElasticModulus;
        public double RElasticModulus;
        public double TElasticModulus;

        public Material(double LExp, double RExp, double TExp, double LElast, double RElast, double TElast)
        {
            LExpansion = LExp;
            RExpansion = RExp;
            TExpansion = TExp;
            LElasticModulus = LElast;
            RElasticModulus = RElast;
            TElasticModulus = TElast;
        }
    }
}