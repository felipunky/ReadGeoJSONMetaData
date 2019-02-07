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

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ReadGeoJSONMetaData
{
    public class ReadGeoJSONMetaDataComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadGeoJSONMetaDataComponent()
          : base("ReadGeoJSONMetaData", "ReadGJSONMeta",
              "Reads the fields of the layer from a GEOJSON",
              "THR34D5Workshop", "ExtractData")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddTextParameter( "PathToFile", "Path", "Path to directory where the grib2 file resides", GH_ParamAccess.item );
 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter( "CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item );
            pManager.AddIntegerParameter( "NumberOfFeatures", "NumFeatures", "Outputs the number of features in the layer", GH_ParamAccess.item );

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

            //var ds = OSGeo.GDAL.Gdal.OpenEx( @"D:\SSS\GIS\Nucleos\Nucleos_1549125498284.geojson", 0, null, null, null );
            var driver = OSGeo.OGR.Ogr.GetDriverByName( "GeoJSON" );
            var ds = driver.Open( @"D:\SSS\GIS\Nucleos\Nucleos_1549125498284.geojson", 0 );

            long numberOfFeatures = 0;
            
            var layer = ds.GetLayerByIndex( 0 );

            numberOfFeatures = unchecked( ( int ) layer.GetFeatureCount( 0 ) );
            
            DA.SetData( 0, output );
            DA.SetData( 1, numberOfFeatures );

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
