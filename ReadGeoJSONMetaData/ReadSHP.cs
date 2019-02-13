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

            pManager.AddTextParameter("PathToFile", "Path", "Path to directory where the GeoJSON file resides", GH_ParamAccess.item);
            pManager.AddIntegerParameter("FieldToSelect", "Field", "Input the field you want to select from", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter("CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumberOfFeatures", "NumFeatures", "Outputs the number of features in the layer", GH_ParamAccess.item);
            pManager.AddTextParameter("Fields", "Flds", "Gets the fields in the layer", GH_ParamAccess.list);
            pManager.AddPointParameter("Points of polygons", "Points", "Gets the points that compose a polygon", GH_ParamAccess.tree);
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
            var ds = driver.Open( input, 0 );

            long numberOfFeatures = 0;
            int fieldCount = 0;
            
            var layer = ds.GetLayerByIndex( 0 );
            var layerDefinition = layer.GetLayerDefn();

            numberOfFeatures = unchecked( ( int ) layer.GetFeatureCount( 0 ) );
            fieldCount = layerDefinition.GetFieldCount();

            var fields = new List<string>();

            for( int i = 0; i < fieldCount; ++i )
            {
                
                fields.Add( layerDefinition.GetFieldDefn(i).GetName() );

            }

            int fieldToSelect = 0;
            DA.GetData( 1, ref fieldToSelect );
            
            var pointOut = new Grasshopper.DataTree<Point3d>();
            var fieldOut = new Grasshopper.DataTree<double>();

            for( int i = 0; i < numberOfFeatures; ++i )
            {

                var path = new GH_Path( i );
                var feature = layer.GetFeature( i );
                var geo = feature.GetGeometryRef();
                var ring = geo.GetGeometryRef( 0 );
                int pointCount = ring.GetPointCount();
                fieldOut.Add( feature.GetFieldAsDouble( fieldToSelect ), path );

                for ( int j = 0; j < pointCount; ++j )
                {
           
                    double[] pointList = { 0, 0, 0 };
                    ring.GetPoint( j, pointList );
                    pointOut.Add( new Point3d( pointList[0], pointList[1], pointList[2] ), path );
                    
                }

            }
            
            DA.SetData( 0, output );
            DA.SetData( 1, numberOfFeatures );
            DA.SetDataList( 2, fields );
            DA.SetDataTree( 3, pointOut );
            DA.SetDataTree( 4, fieldOut );

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