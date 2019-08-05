using System;
using System.Threading;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace ReadGeoJSONMetaData
{
    public class ReadSHP : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReadSHP class.
        /// </summary>
        public ReadSHP()
          : base("ReadShapefile", "ReadSHP",
              "Reads a shapefile",
              "THR34D5Workshop", "ExtractData")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddTextParameter("Path to file", "Path", "Path to directory where the Shapefile file resides", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Parallel computation", "Parallel", "If the data is too large the computation may benefit from parallelization", GH_ParamAccess.item);
            pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter("CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item);
            pManager.AddNumberParameter("Latitude of polygons", "Latitude", "Gets the latitudes that compose a polygon", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Longitude of polygons", "Longitude", "Gets the longitude that compose a polygon", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // See if gdal and ogr are working first.
            string output = "";

            try
            {

                GdalConfiguration.ConfigureOgr();

                GdalConfiguration.ConfigureGdal();

                output = "It works!";

            }

            catch ( Exception e )
            {

                output = "{0} Exception caught. " + e;

            }
            
            string input = "";

            DA.GetData( 0, ref input );

            var driver = OSGeo.OGR.Ogr.GetDriverByName( "ESRI Shapefile" );

            if( driver == null )
            {

                output = "Driver is null";

            }

            else
            { 

                var ds = driver.Open( input, 0 );

                if( ds == null )
                {

                    output = "DataSource is null";

                }

                else
                {
                   
                    Grasshopper.DataTree<double> latitudesOut = new Grasshopper.DataTree<double>(), longitudesOut = new Grasshopper.DataTree<double>();

                    long numberOfFeatures = 0;

                    var layer = ds.GetLayerByIndex(0);

                    string geoType = layer.GetGeomType().ToString();

                    numberOfFeatures = unchecked((int)layer.GetFeatureCount(0));

                    bool parallel = false;

                    DA.GetData( 1, ref parallel );

                    GH_Path path = new GH_Path();

                    OSGeo.OGR.Feature feature = null;
                    OSGeo.OGR.Geometry geo = null, ring = null;

                    if( parallel == false )
                    {

                        if( geoType == "wkbPolygon" )
                        {

                            for( int i = 0; i < numberOfFeatures; ++i )
                            {

                                path = new GH_Path(i);
                                feature = layer.GetFeature(i);
                                geo = feature.GetGeometryRef();
                                ring = geo.GetGeometryRef(0);
                                int pointCount = ring.GetPointCount();

                                for (int j = 0; j < pointCount; ++j)
                                {

                                    double[] pointList = { 0, 0 };
                                    ring.GetPoint_2D(j, pointList);
                            
                                    latitudesOut.Add( pointList[1], path );
                                    longitudesOut.Add( pointList[0], path );

                                }

                            }

                        }

                        else if( geoType == "wkbLineString" )
                        {

                            for( int i = 0; i < numberOfFeatures; ++i )
                            {

                                path = new GH_Path(i);
                                feature = layer.GetFeature(i);
                                geo = feature.GetGeometryRef();
                                int pointCount = geo.GetPointCount();

                                for (int j = 0; j < pointCount; ++j)
                                {

                                    double[] pointList = { 0, 0 };
                                    geo.GetPoint_2D(j, pointList);
                            
                                    latitudesOut.Add( pointList[1], path );
                                    longitudesOut.Add( pointList[0], path );

                                }

                            }

                        }

                        else
                        {

                            output = "Geometry type not implemented yet";

                        }

                    }

                    else
                    {

                        double[][] latMat = new double[numberOfFeatures][];
                        double[][] lonMat = new double[numberOfFeatures][];

                        if( geoType == "wkbPolygon" )
                        { 

                            for( int i = 0; i < numberOfFeatures; ++i )
                            {

                                path = new GH_Path(i);
                                feature = layer.GetFeature(i);
                                geo = feature.GetGeometryRef();
                                ring = geo.GetGeometryRef(0);
                                int pointCount = ring.GetPointCount();

                                latMat[i] = new double[pointCount];
                                lonMat[i] = new double[pointCount];
                            
                                System.Threading.Tasks.Parallel.For(0, pointCount, j =>
                                {

                                    double[] pointList = { 0, 0 };
                                    ring.GetPoint_2D(j, pointList);

                                    latMat[i][j] = pointList[1];
                                    lonMat[i][j] = pointList[0];

                                });

                                if( pointCount > 0 )
                                {

                                    latitudesOut.AddRange( latMat[i], path );
                                    longitudesOut.AddRange( lonMat[i], path );

                                }

                            }

                        }

                        else if( geoType == "wkbLineString" )
                        {
                            
                            for( int i = 0; i < numberOfFeatures; ++i )
                            {

                                path = new GH_Path(i);
                                feature = layer.GetFeature(i);
                                geo = feature.GetGeometryRef();
                                int pointCount = geo.GetPointCount();

                                latMat[i] = new double[pointCount];
                                lonMat[i] = new double[pointCount];

                                System.Threading.Tasks.Parallel.For( 0, pointCount, j => 
                                {

                                    double[] pointList = { 0, 0 };
                                    geo.GetPoint_2D( j, pointList );

                                    latMat[i][j] = pointList[1];
                                    lonMat[i][j] = pointList[0];

                                });

                                if( pointCount > 0 )
                                {

                                    latitudesOut.AddRange( latMat[i], path );
                                    longitudesOut.AddRange( lonMat[i], path );

                                }

                            }

                        }

                        else
                        {

                            output = "Geometry type not implemented yet";

                        }

                    }
                    
                    DA.SetDataTree( 1, latitudesOut );
                    DA.SetDataTree( 2, longitudesOut );

                }

            }

            DA.SetData( 0, output );

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
            get { return new Guid("a2e3c03b-0df9-4538-badd-0070388622a2"); }
        }
    }
}