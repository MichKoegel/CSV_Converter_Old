using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV_Converter.Geometries
{
    class Cone : BaseGeometry
    {
        /**
         * virtual type specific function for the calculation of the output parameter
         */
        protected override bool CalculateOutputParameter(out string error)
        {
            error = "";
            /* check if the required data is available */
            foreach (string key in new string[] { "X", "Y", "Z", "i", "j", "k" })
            {
                if (!m_doubleData.ContainsKey(key))
                {
                    error = $"Error: Unable to calculate output parameters of the cone since the value for \"{key}\" is missing.";
                    return false;
                }
            }

            double value;
            if (m_doubleData.TryGetValue("halfangle", out value)) // half angle input
            {
                m_doubleData["Var1"] = 2.0 * value;
            }
            else if (m_doubleData.TryGetValue("angle", out value))
            { // angle input
                m_doubleData["Var1"] = value;
                /* add halfangle. Might be necessary for further calculations. */
                m_doubleData["halfangle"] = 0.5 * value; 
            }
            else
            {
                error = "Error: Unable to calculate Var1 from the input data";
                return false;
            }

            /* handle the different input cases h1 <-> r2 */
            string stringh1;
            bool validH1 = false;
            if (m_stringData.TryGetValue("h1", out stringh1))
            {
                double h1 = new double();
                /* check if the value is valid */
                validH1 = !String.IsNullOrWhiteSpace(stringh1);
                try
                {
                    h1 = Convert.ToDouble(stringh1);
                }
                catch (Exception ex)
                {
                    error = $"error converting \"{stringh1}\" to double due to: " + ex.Message;
                    validH1 = false;
                }

                if (validH1)
                {
                    /* case: h1 is given and a valid double value
                       h1, the halfangle, the orientation (i,j,k) and (X,Y,Z) at the apex define a distinct cone.
                       It is not possible to calculate h2 without further info like a relation between h1&h2 or r1&r2 */
                    m_doubleData["Attr1"] = h1;
                }
               
            }
            if (!validH1)
            {
                string stringr2;
                if (m_stringData.TryGetValue("r2", out stringr2))
                {
                    double r2 = new Double();
                    /* check if the value is valid */
                    bool validr2 = !String.IsNullOrWhiteSpace(stringr2);
                    if (validr2)
                    {
                        try
                        {
                            r2 = Convert.ToDouble(stringr2);
                        }
                        catch (Exception ex)
                        {
                            error = $"error converting \"{stringr2}\" to double due to: " + ex.Message;
                            validr2 = false;
                        }
                    }
                    if (validr2)
                    {
                        /* case: r2 is given and a valid double
                           r2, the halfangle, the orientation (i,j,k) and the center position (X,Y,Z) at the height h2 where the cone has radius r2 define a distinct cone with height h2.
                           It is possible to calculate the position of the apex.
                           It is not possible to calculate h1 without further info like a relation between h1&h2 or r1&r2.
                         */

                        /* using r=h*tan(halfangle)*/
                        double tanA = 0.0;
                        try
                        {
                            tanA = Math.Tan(m_doubleData["halfangle"] * Math.PI / 180.0);
                            if (tanA == Double.NaN || Math.Abs(tanA) < 1e-12)
                            {
                                error = "Error: invalid angle input. Tangent is NaN or near zero ";
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            error = "Error: invalid angle input due to: " + ex.Message;
                            return false;
                        }
                       
                        double h2 = r2 / tanA;
                        m_doubleData["Var2"] = h2;
                        /* cone top = position + h2 in the normed direction from bottom -> top */
                        double directionNorm = Math.Sqrt(m_doubleData["i"] * m_doubleData["i"] + // just in case that the input is not normed
                                                         m_doubleData["j"] * m_doubleData["j"] +
                                                         m_doubleData["k"] * m_doubleData["k"]);
                        m_doubleData["X"] += (h2 * m_doubleData["i"]) / directionNorm;
                        m_doubleData["Y"] += (h2 * m_doubleData["j"]) / directionNorm;
                        m_doubleData["Z"] += (h2 * m_doubleData["k"]) / directionNorm;

                        /* orientation has to be inverted to top -> bottom*/
                        m_doubleData["i"] *= -1.0;
                        m_doubleData["j"] *= -1.0;
                        m_doubleData["k"] *= -1.0;
                    }
                    else
                    {
                        error = "Error: Unable to define a distinct cone with the input data";
                        return false;
                    }
                }
                else
                {
                    error = "Error: Unable to define a distinct cone with the input data";
                    return false;
                }
            }
            return true;
        }

    }
}
