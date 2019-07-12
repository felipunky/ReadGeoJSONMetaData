using System;
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
            pManager.AddIntegerParameter("Field to select", "Field", "Input the field you want to select from", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddIntervalParameter("Restrict range in latitudes", "LatRange", "If you only want an specific range of coordinates from the file, input a domain", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddIntervalParameter("Restrict range in longitudes", "LonRange", "If you only want an specific range of coordinates from the file, input a domain", GH_ParamAccess.item);
            pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter("CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumberOfFeatures", "NumFeatures", "Outputs the number of features in the layer", GH_ParamAccess.item);
            pManager.AddTextParameter("Fields", "Flds", "Gets the fields in the layer", GH_ParamAccess.list);
            pManager.AddNumberParameter("Latitude of polygons", "Latitude", "Gets the latitudes that compose a polygon", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Longitude of polygons", "Longitude", "Gets the longitude that compose a polygon", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Output field", "Field", "Outputs the field information", GH_ParamAccess.tree);

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
                    int fieldCount = 0;

                    int layerIndex = 0;

                    DA.GetData(1, ref layerIndex);

                    var layer = ds.GetLayerByIndex(layerIndex);
                    var layerDefinition = layer.GetLayerDefn();

                    numberOfFeatures = unchecked((int)layer.GetFeatureCount(0));
                    fieldCount = layerDefinition.GetFieldCount();

                    var fields = new List<string>();

                    for (int i = 0; i < fieldCount; ++i)
                    {

                        fields.Add(layerDefinition.GetFieldDefn(i).GetName());

                    }

                    int fieldToSelect = 0;
                    DA.GetData(1, ref fieldToSelect);
                    
                    var fieldOut = new Grasshopper.DataTree<double>();

                    bool flag = false;

                    Interval latRange = new Interval();
                    if( DA.GetData( 2, ref latRange ) )
                    {

                        flag = true;

                    }

                    Interval lonRange = new Interval();
                    if ( DA.GetData( 3, ref lonRange ) )
                    {

                        flag = true;

                    }

                    double tempLat = 0.0, tempLon = 0.0;

                    for (int i = 0; i < numberOfFeatures; ++i)
                    {

                        var path = new GH_Path(i);
                        var feature = layer.GetFeature(i);
                        var geo = feature.GetGeometryRef();
                        var ring = geo.GetGeometryRef(0);
                        int pointCount = ring.GetPointCount();
                        fieldOut.Add(feature.GetFieldAsDouble(fieldToSelect), path);

                        for (int j = 0; j < pointCount; ++j)
                        {

                            double[] pointList = { 0, 0, 0 };
                            ring.GetPoint(j, pointList);

                            tempLat = pointList[0];
                            tempLon = pointList[1];

                            if ( flag == false )
                            {

                                latitudesOut.Add( tempLat, path );
                                longitudesOut.Add( tempLon, path );

                            }

                            else
                            {

                                if( tempLat >= latRange.T0 && tempLat <= latRange.T1 )
                                {

                                    latitudesOut.Add( tempLat, path );

                                }

                                if( tempLon >= lonRange.T0 && tempLon <= lonRange.T1 )
                                {

                                    longitudesOut.Add( tempLon, path );

                                }

                            }

                        }

                    }

                    DA.SetData(1, numberOfFeatures);
                    DA.SetDataList(2, fields);
                    DA.SetDataTree(3, latitudesOut);
                    DA.SetDataTree(4, longitudesOut);
                    DA.SetDataTree(5, fieldOut);

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