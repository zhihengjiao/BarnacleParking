using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Barnacle
{
    public class RowSolvingMain : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        RowSolver res;
        bool getData = false;
        bool calculation = false;
        public RowSolvingMain()
          : base("Barnacle", "Barnacle",
              "Description",
              "Barnacle", "Warppers")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("point a", "a", "point of the polygon", GH_ParamAccess.item);
            pManager.AddPointParameter("point b", "b", "point of the polygon", GH_ParamAccess.item);
            pManager.AddPointParameter("point c", "c", "point of the polygon", GH_ParamAccess.item);
            pManager.AddPointParameter("point d", "d", "point of the polygon", GH_ParamAccess.item);
            pManager.AddIntegerParameter("top i result", "i", "the ith best result", GH_ParamAccess.item);
            pManager.AddBooleanParameter("get data", "bool", "set ture at beginning", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("result", "res", "Geometry of parking lot", GH_ParamAccess.list);
            pManager.AddIntegerParameter("measurement", "metric", "Count of parking lot", GH_ParamAccess.item);
            // pManager.AddTextParameter("log", "log", "print of result", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool e = false;
            if (!DA.GetData(5, ref e))
            {
                return;
            }

            Point3d a = new Point3d();
            Point3d b = new Point3d();
            Point3d c = new Point3d();
            Point3d d = new Point3d();
            int i = 0;

            if (!DA.GetData(0, ref a)) return;
            if (!DA.GetData(1, ref b)) return;
            if (!DA.GetData(2, ref c)) return;
            if (!DA.GetData(3, ref d)) return;
            DA.GetData(4, ref i);

            Point3d[] points = new Point3d[] { a, b, c, d };

            if (e)
            {
                Zone zone = new Zone(points);

                RowSolver solver = new RowSolver(new StallCountMetric(), new RowSolverResult());
                solver.WithZone(zone);
                solver.Solve();
                res = solver;
            }
            
            if (res != null)
            {
                DA.SetDataList(0, res.GetBest(i).Draw());
                DA.SetData(1, res.GetBest(i).CalculateTotalStall());
            } else
            {

            }
           
            // DA.SetData(1, solver.log);
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cb40292c-7f57-4b55-9b94-8798789e4abc"); }
        }
    }
}
