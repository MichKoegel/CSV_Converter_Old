using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV_Converter.Geometries
{
    class Plane : BaseGeometry
    {
        /**
         * virtual type specific function for the calculation of the output parameter
         */
        protected override bool CalculateOutputParameter(out string error)
        {
            /* nothing to calculate for this geometry */
            error = "";
            return true;
        }


    }
}
