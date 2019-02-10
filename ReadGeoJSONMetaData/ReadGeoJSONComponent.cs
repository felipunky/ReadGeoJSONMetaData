using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using GH_IO;
using Grasshopper.Kernel.Data;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ReadGeoJSONMetaData
{
    public class ReadGeoJSONComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadGeoJSONComponent()
          : base("ReadGeoJSON", "ReadGJSON",
              "Reads the fields of the layer from a GEOJSON",
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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
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

            var driver = OSGeo.OGR.Ogr.GetDriverByName( "GeoJSON" );
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
            
            var pointOut = new DataTree<Point3d>();
            var fieldOut = new DataTree<double>();

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
            get { return new Guid("a9f27db7-39c2-4be5-9bc5-28ab7e08f6de"); }
        }
    }
}
