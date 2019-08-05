using System;
using System.Threading;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace ReadGeoJSONMetaData
{
    public class ShapeFileFieldExtractor : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ShapeFileFieldExtractor class.
        /// </summary>
        public ShapeFileFieldExtractor()
          : base("ShapeFileFieldExtractor", "SHPFields",
              "Extracts the desired field from a SHP file",
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
            /*pManager.AddBooleanParameter("Parallel computation", "Parallel", "If the data is too large the computation may benefit from parallelization", GH_ParamAccess.item);
            pManager[2].Optional = true;*/

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter("CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumberOfFeatures", "NumFeatures", "Outputs the number of features in the layer", GH_ParamAccess.item);
            pManager.AddTextParameter("Fields", "Flds", "Gets the fields in the layer", GH_ParamAccess.list);
            pManager.AddGenericParameter("Output field", "Field", "Outputs the field information", GH_ParamAccess.list);
            pManager.AddTextParameter("Geometry type", "GeoType", "Outputs the geometry type of the Shapefile", GH_ParamAccess.item);

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

                    long numberOfFeatures = 0;
                    int fieldCount = 0;

                    int fieldSelector = 0;

                    DA.GetData( 1, ref fieldSelector );

                    var layer = ds.GetLayerByIndex(0);

                    string geoType = layer.GetGeomType().ToString();

                    var layerDefinition = layer.GetLayerDefn();

                    numberOfFeatures = unchecked((int)layer.GetFeatureCount(0));
                    fieldCount = layerDefinition.GetFieldCount();

                    string[] fields = new string[fieldCount];

                    object[] fieldOut = new object[numberOfFeatures];

                    for( int i = 0; i < fieldCount; ++i )
                    {

                        fields[i] = layerDefinition.GetFieldDefn(i).GetName();

                    }

                    int fieldToSelect = 0;
                    DA.GetData( 1, ref fieldToSelect );

                    //bool parallel = false;

                    //DA.GetData( 2, ref parallel );

                    OSGeo.OGR.Feature feature = null;

                    //if( parallel == false )
                    {

                        for( int i = 0; i < numberOfFeatures; ++i )
                        {
                            
                            feature = layer.GetFeature(i);
                            fieldOut[i] = feature.GetFieldAsString( fieldSelector );

                        }

                    }

                    /*else
                    {

                        try
                        {

                            object locker = new object();

                            System.Threading.Tasks.Parallel.For( 0, numberOfFeatures, i => 
                            {

                                feature = layer.GetFeature(i);

                                lock( locker )
                                { 

                                    fieldOut[i] = feature.GetFieldAsString( fieldSelector );

                                }

                                //feature = null;

                            });

                        }

                        catch( Exception e )
                        {

                            output = "{0} Exception caught. " + e;

                        }

                    }*/

                    DA.SetData( 1, numberOfFeatures );
                    DA.SetDataList( 2, fields );
                    DA.SetDataList( 3, fieldOut );
                    DA.SetData( 4, geoType );

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
            get { return new Guid("756e3509-48a9-43a6-b2a3-4266d549c3dc"); }
        }
    }
}