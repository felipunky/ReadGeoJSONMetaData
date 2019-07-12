using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ReadGeoJSONMetaData
{
    public class GetDrivers : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetDrivers class.
        /// </summary>
        public GetDrivers()
          : base("GetDrivers", "Drivers",
              "Gets the index and names of the drivers provided by Ogr",
              "THR34D5Workshop", "ExtractData")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddBooleanParameter( "Run It?", "Run", "Run it or not", GH_ParamAccess.item );

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter( "CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item );
            pManager.AddTextParameter( "Name of the drivers", "Name", "Returns the name of the drivers", GH_ParamAccess.list );

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<string> driverName = new List<string>();

            bool run = false;

            DA.GetData( 0, ref run );

            // See if gdal and ogr are working first.
            string output = "";

            try
            {

                GdalConfiguration.ConfigureOgr();

                GdalConfiguration.ConfigureGdal();

                output = "It works!";

            }

            catch (Exception e)
            {

                output = "{0} Exception caught. " + e;

            }

            if( run )
            {

                int driverCount = OSGeo.OGR.Ogr.GetDriverCount();
                //driverName.Add( driverCount.ToString() );

                for (int i = 0; i < driverCount; ++i)
                {

                    driverName.Add(OSGeo.OGR.Ogr.GetDriver(i).GetName());

                }

            }

            DA.SetData( 0, output );

            DA.SetDataList( 1, driverName );
            
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
            get { return new Guid("b7e85fad-568e-4774-849c-ed63d85efc75"); }
        }
    }
}