using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;
using System.Linq;

namespace HygroDesign.Grasshopper.Update
{

    public class DefineRadiusBlending : GH_Component
    {

        public DefineRadiusBlending()
          : base("Radius Blending", "Rad Blend",
            "Update a panel's active boards and HMaxels radius prediciton through blending with neighboring radii.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The panels to update.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Convolution Engine", "CE", "The convolution engine defining the blending behavior.", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panels", "P", "The updated panels.", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Panel> oldPanels = new List<Panel>();
            DA.GetDataList(0, oldPanels);
            List<Panel> panels = new List<Panel>();
            foreach (Panel panel in oldPanels)
            {
                panels.Add(Panel.DeepCopy(panel));
            }
            ConvolutionEngine convolutionEngine = null;
            DA.GetData(1, ref convolutionEngine);


            
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("38641671-1AF0-4EAE-A6A4-B2155F2D5021");
    }
}