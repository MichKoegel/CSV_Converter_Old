using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace CSV_Converter
{
    /**
     * The base class for all geometries.
     * Defines the base interface for creating a geometry and creating an output string describing the geometry.
     */ 
    class BaseGeometry
    {
        /**
         * Constructor
         */ 
        public BaseGeometry()
        {
            m_stringData = new Dictionary<string, string>();
            m_doubleData = new Dictionary<string, double>();
        }

        /**
         * Create the geometry with its mapping of the input data based on the type specific configuration input & output settings.
         */
        public bool CreateGeometry(List<string> inputData, InputSettings inputSettings, OutputSettings outputSettings, out string error)
        {
            error = "";
            try
            {
                if ((inputData.Count < inputSettings.Names.Count) || (inputData.Count < inputSettings.Types.Count))
                {
                    error = "Error: Less input data than expected during the mapping of the input data.";
                    return false;
                }

                if ((outputSettings.Names.Count != outputSettings.Types.Count) ||
                    (outputSettings.Names.Count != outputSettings.DefVals.Count) ||
                    (outputSettings.Names.Count != outputSettings.DecimalDigits.Count))
                {
                    error = "Error: output setting sizes are not matching.";
                    return false;
                }

                /* map the input values to the corresponding names */
                for (int idx = 0; idx < inputSettings.Names.Count; idx++)
                {
                    if (m_stringData.ContainsKey(inputSettings.Names.ElementAt(idx)) || m_doubleData.ContainsKey(inputSettings.Names.ElementAt(idx)))
                    {
                        error = $"Error: Input name mapping not unique due to {inputSettings.Names.ElementAt(idx)}";
                        return false;
                    }

                    if (inputSettings.Types.ElementAt(idx) == "double")
                    {
                        Double value = new Double();
                        try
                        {
                            value = Convert.ToDouble(inputData.ElementAt(idx));
                        }
                        catch (Exception ex)
                        {
                            error = $"Error: Converting \"{inputData.ElementAt(idx)}\" to double due to: " + ex.Message;
                            return false;
                        }

                        m_doubleData.Add(inputSettings.Names.ElementAt(idx), value);
                    }
                    else /* type is "string" */
                    {
                        m_stringData.Add(inputSettings.Names.ElementAt(idx), inputData.ElementAt(idx));
                    }
                }

                /* save the output settings for later usage */
                m_OutputSettings = outputSettings;
                return true;
            }
            catch (Exception ex)
            {
                error = "Error: Unable to create geometry due to " + ex.Message;
                return false;
                ;
            }
        }

        /**
         * Create the CALIGO conform formated string output for the geometry.
         * This function calls a type specific internal function for the type specific calculation of possibly missing output parameter.
         */
        public bool CreateOutput(int objectId, out List<string> outputList, out string error)
        {
            error = "";
            outputList = new List<string>();

            if (m_OutputSettings == null)
            {
                error = "Error: Geometry was not created correctly.";
                return false;
            }

            /* m_OutputSettings != null implies that all its Lists have the same size */

            /* calculate the output parameter type specific and possibly also case specific */
            if (!CalculateOutputParameter(out error))
            {
                return false;
            }

            string outputValue;
            double value;
            for (int idx = 0; idx < m_OutputSettings.Value.Names.Count; idx++)
            {

                switch (m_OutputSettings.Value.Types[idx])
                {
                    case "string":
                        {
                            if (!m_stringData.TryGetValue(m_OutputSettings.Value.Names[idx], out outputValue))
                            {
                                /* missing string values are set to their default values */
                                outputValue = m_OutputSettings.Value.DefVals[idx];
                            }
                            break;
                        }
                    default: /* double is the default type */
                        {
                            if (m_doubleData.TryGetValue(m_OutputSettings.Value.Names[idx], out value))
                            {
                                /* create output string with correct decimal settings and format */
                                string decimalSettings = "F0" + m_OutputSettings.Value.DecimalDigits[idx].ToString();
                                outputValue = String.Format("{0}", value.ToString(decimalSettings, CultureInfo.CreateSpecificCulture("en-GB")));
                            }
                            else
                            {
                                /* values that are missing after the type specific calculation are set to their default value */
                                if (String.IsNullOrWhiteSpace(m_OutputSettings.Value.DefVals[idx]))
                                {
                                    outputValue = m_OutputSettings.Value.DefVals[idx];
                                }
                                else // non trivial default value
                                {
                                    try
                                    {
                                        if (Double.TryParse(m_OutputSettings.Value.DefVals[idx], out value))
                                        {
                                            /* create output string with correct decimal settings and format */
                                            string decimalSettings = "F0" + m_OutputSettings.Value.DecimalDigits[idx].ToString();
                                            outputValue = String.Format("{0}", value.ToString(decimalSettings, CultureInfo.CreateSpecificCulture("en-GB")));
                                        }
                                        else
                                        {
                                            error = $"Error: Unable to convert \"{m_OutputSettings.Value.DefVals[idx]}\" to double";
                                            return false;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        error = $"Error: Unable to convert \"{m_OutputSettings.Value.DefVals[idx]}\" to double due to: " + ex.Message;
                                        return false;
                                    }
                                }
                            }
                            break;
                        }
                }

                outputList.Add(outputValue);
            }
            return true;
        }

        /**
         * Get the feature name of this geometry
         */
        public bool GetGeometryName(out string name, out string error)
        {
            error = "";
            name = "";
            if (m_OutputSettings == null)
            {
                error = "Error: Unable to get the geometry name. Geometry was not created correctly.";
                return false;
            }
            else
            {
                name = m_OutputSettings.Value.FeatureName;
                return true;
            }
        }

        public void UpdateGeometryName(string name)
        {
            /* add/overwrite the updated feature name to the parameter mapping */
            m_stringData["featurename"] = name;
        }

        /**
         * virtual type specific function for the calculation of the output parameter
         */
        protected virtual bool CalculateOutputParameter(out string error)
        {
            error = "nothing calculated";
            return true;
        }

        /* members */
        protected Dictionary<string, string> m_stringData;         //!< mapping of the string input data and their corresponding keys
        protected Dictionary<string, double> m_doubleData;         //!< mapping of the double input data and their corresponding keys
        protected OutputSettings? m_OutputSettings;                //!< the output configuration settings of this geometry
    }
}
