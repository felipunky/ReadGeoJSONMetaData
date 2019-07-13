using System;
using System.Collections.Generic;
using System.Threading;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ReadGeoJSONMetaData
{
    public class WebMercatorProjection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WebMercatorProjection class.
        /// </summary>
        public WebMercatorProjection() : base("WebMercator", "WebMercatorProjection", "This component converts latitudes and longitudes into a WebMercator projection", "THR34D5Workshop", "Projections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Parallel computing", "Parallel", "If there are too many points speed can be increased through parallelization", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddNumberParameter("Latitude", "Lat", "Input here the latitudes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Longitude", "Lon", "Input here the longitudes", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("WebMercator Projection", "WbMctrPrj", "Transformed latitudes and longitudes to points on a WebMercator 2D projection", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> latitudes = new List<double>();
            List<double> longitudes = new List<double>();
            DA.GetDataList<double>(2, latitudes);
            DA.GetDataList<double>(1, longitudes);

            int size = latitudes.Count;

            Point3d[] pointsOut = new Point3d[size];

            bool parallel = false;
            DA.GetData(0, ref parallel);

            if (parallel == false)
            {

                for (int i = 0; i < size; i++)
                {

                    pointsOut[i] = this.MercatorProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                }

            }

            else
            {

                System.Threading.Tasks.Parallel.For(0, latitudes.Count, i =>
                {

                    pointsOut[i] = this.MercatorProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                });

            }

            DA.SetDataList(0, pointsOut);

        }

        public Point3d MercatorProjection(double lat, double lon)
        {
            int num = 6367;
            double num2 = Math.Sin(lon);
            double x = lat * (double)num;
            double y = (double)num * 0.5 * Math.Log((1.0 + num2) / (1.0 - num2));
            return new Point3d(x, y, 0.0);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("AD1B6F80-A3AA-4BC7-A921-CA0FC6CBBC3B");
            }
        }
    }
}