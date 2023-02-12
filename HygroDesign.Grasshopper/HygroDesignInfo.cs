using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace HygroDesign
{
    public class HygroDesignInfo : GH_AssemblyInfo
    {
        public override string Name => "HygroDesign";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "A set of tools for designing architectural structures with self shaping wood.";

        public override Guid Id => new Guid("185919CB-6F37-4714-84D6-C6A28CEA9001");

        //Return a string identifying you or your company.
        public override string AuthorName => "Alex Reiner, Edgar Schefer, Aaron Wagner";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}