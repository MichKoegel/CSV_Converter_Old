using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSV_Converter.Geometries
{
    class Circle : BaseGeometry
    {
        /**
         * virtual type specific function for the calculation of the output parameter
         */
        protected override bool CalculateOutputParameter(out string error)
        {
            error = "";
            /* calculate diameter if missing */
            double value;
            if (m_doubleData.TryGetValue("radius", out value)) // radius input
            {
                m_doubleData["Var1"] = 2.0 * value;
            }
            else if (m_doubleData.TryGetValue("diam", out value))
            { // diameter input
                m_doubleData["Var1"] = value;
            }
            else
            {
                error = "Error: Unable to calculate Var1 from the input data";
                return false;
            }

            return true;
        }
    }
}
