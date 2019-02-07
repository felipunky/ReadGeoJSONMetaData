using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ReadGeoJSONMetaData
{
    public class ReadGeoJSONMetaDataInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ReadGeoJSONMetaData";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "This component allows to extract what information the GeoJSON contains";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("EC6DD934-F32A-4689-8BCF-E3335778A308");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Felipe Gutierrez Duque";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "felipegutierrezduque10@gmail.com";
            }
        }
    }
}
