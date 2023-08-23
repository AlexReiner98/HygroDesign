using BilayerDesign;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino;


namespace HygroDesign.Grasshopper.Update
{

    public class DefineUpdatePanel : GH_Component
    {

        public DefineUpdatePanel()
          : base("Update Panel", "Up Pan",
            "Update a panel with bilayers and HMaxels.",
            "HygroDesign", "Design")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The panel to update.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Bilayers", "B", "The bilayers to add to the panel.", GH_ParamAccess.list);
            pManager.AddGenericParameter("HMaxels", "HM", "The HMaxels to replace on the panel.", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Panel", "P", "The updated panel.", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Panel oldPanel = null;
            DA.GetData(0, ref oldPanel);
            Panel panel = Panel.DeepCopy(oldPanel);

            List<Bilayer> bilayers = new List<Bilayer>();
            DA.GetDataList(1, bilayers);

            List<HMaxel> hmaxels = new List<HMaxel>();
            DA.GetDataList(2, hmaxels);

            panel.Bilayers = bilayers;
            for(int i = 0; i < panel.Bilayers.Count; i++) 
            {
                panel.Bilayers[i].ID = i;
            }

            foreach(HMaxel hmaxel in hmaxels)
            {
                panel.HMaxels[hmaxel.I,hmaxel.J] = hmaxel;
            }

            DA.SetData(0, panel);
        }



        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("600457C6-06B8-4624-82D4-EE4CA2F972CC");
    }
}