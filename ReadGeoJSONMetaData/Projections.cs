using System;
using System.Collections.Generic;
using System.Threading;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ReadGeoJSONMetaData
{
    public class Projections : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WebMercatorProjection class.
        /// </summary>
        public Projections() : base("Projections", "Proj", "This component converts latitudes and longitudes into different projections", "THR34D5Workshop", "Projections")
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
            pManager.AddIntegerParameter("Projection", "Proj", "0 for Web Mercator projection, 1 for Tranverse Mercator projection, 2 for Spherical projection, 3 for Cassini projection, defaults to Web Mercator ", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddPointParameter("Projected Points", "ProjPts", "Transformed latitudes and longitudes", GH_ParamAccess.list);
            pManager.AddTextParameter("Erros", "Err", "Outputs errors", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string output = "";

            List<double> latitudes = new List<double>();
            List<double> longitudes = new List<double>();
            DA.GetDataList<double>(2, latitudes);
            DA.GetDataList<double>(1, longitudes);

            int size = latitudes.Count;

            Point3d[] pointsOut = new Point3d[size];

            bool parallel = false;
            DA.GetData(0, ref parallel);

            int proj = 0;
            DA.GetData(3, ref proj);

            proj %= 4;

            if (parallel == false)
            {

                switch (proj)
                {

                    case 0:

                        for (int i = 0; i < size; i++)
                        {

                            pointsOut[i] = WebMercatorProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        }

                        break;

                    case 1:

                        /*for (int i = 0; i < size; i++)
                        {

                            pointsOut[i] = WebMercatorProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        }*/

                        output = "Not implemented yet";

                        break;

                    case 2:

                        for (int i = 0; i < size; i++)
                        {

                            pointsOut[i] = SphericalProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        }

                        break;

                    case 3:

                        for (int i = 0; i < size; i++)
                        {

                            pointsOut[i] = CassiniProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        }

                        break;

                }

            }

            else
            {

                switch (proj)
                {

                    case 0:

                        System.Threading.Tasks.Parallel.For(0, latitudes.Count, i =>
                        {

                            pointsOut[i] = WebMercatorProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        });

                        break;

                    case 1:

                        output = "Not implemented yet";

                        break;

                    case 2:

                        System.Threading.Tasks.Parallel.For(0, latitudes.Count, i =>
                        {

                            pointsOut[i] = SphericalProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        });

                        break;

                    case 3:

                        System.Threading.Tasks.Parallel.For(0, latitudes.Count, i =>
                        {

                            pointsOut[i] = CassiniProjection(Rhino.RhinoMath.ToRadians(latitudes[i]), Rhino.RhinoMath.ToRadians(longitudes[i]));

                        });

                        break;

                }
                
            }

            DA.SetDataList(0, pointsOut);

            DA.SetData(1, output);

        }

        public Point3d WebMercatorProjection(double lat, double lon)
        {
            double radius= 6378.0; 
            double FSin = Math.Sin(lon);
            double x = lat * radius;
            double y = radius * 0.5 * Math.Log((1.0 + FSin) / (1.0 - FSin));
            return new Point3d(x, y, 0.0);
        }

        public Point3d CassiniProjection(double lat, double lon)
        {
            double num = 6378.0;
            double x = num * Math.Asin(Math.Cos(lat) * Math.Sin(lon));
            double y = num * Math.Atan2(Math.Sin(lat), Math.Cos(lat) * Math.Cos(lon));
            double num2 = num * Math.Sin(lat);
            return new Point3d(x, y, 0.0);
        }

        public Point3d SphericalProjection(double lat, double lon)
        {
            double num = 6378.0;
            double x = num * Math.Cos(lon) * Math.Cos(lat);
            double y = num * Math.Cos(lon) * Math.Sin(lat);
            double z = num * Math.Sin(lon);
            return new Point3d(x, y, z);
        }

        /*public Point3d TranverseMercator(double lat, double lon)
        {
            double radius = 6378.0;
            double B = Math.Cos( lat ) * Math.Sin( long -  )
            double x = 0.5 * Math.Log(  )
        }*/

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