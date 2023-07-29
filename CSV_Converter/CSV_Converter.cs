using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CSV_Converter.Geometries;


namespace CSV_Converter
{
    /**
     * class for the conversion of geometry data from a more or less arbitrary CSV format into a CALIGO conform format.
     * 
     */
    class CSVConverter
    {
        /**
         * Initializes and configurates the class.
         */ 
        public bool Init(out string error)
        {
            error = "";
            m_Geometries = new List<BaseGeometry>();
            m_NameCounter = new Dictionary<string, int>();
            m_InputSettings = new Dictionary<string, InputSettings>();
            m_OutputSettings = new Dictionary<string, OutputSettings>();
            return ReadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml"), out error);
        }

        /**
         * open a text file.
         * Parse the content of each line and try creating a geometry with that data.
         */ 
        public bool OpenAndParse(string filename, out string error)
        {
            error = "";

            try
            {
                /* reset internal geometry data */
                m_NameCounter.Clear();
                m_Geometries.Clear();

                var lines = System.IO.File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    /* create internal geometry data with the given line data */
                    if (!AddGeometry(line, out error))
                    {
                        error = $"Error: Creating geometrie with the input: \"{line}\" failed due to: " + error;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                error = $"Error: Opening and parsing the file failed due to: " + ex.Message;
                return false;
            }

            return true;
        }

        /**
         * Fill a text file with the data of the internal geometries in a CALIGO conform format.
         */
        public bool Convert(string filename, string map, string model, string user, string name, out string error)
        {
            error = "";
            try
            {
                /* create the file */
                StreamWriter streamWriter = new StreamWriter(filename, false);
                /* write header to the file */
                streamWriter.WriteLine("MAP: " + map);
                streamWriter.WriteLine("MODEL: " + model);
                streamWriter.WriteLine(String.Format("USER: {0}   NAME: {1}   DATE: {2}", user, name, DateTime.Now));
                for (int cnt = 3; cnt < 10; cnt++)
                {
                    streamWriter.WriteLine("-");
                }
                /* create and write the output of the internal geometries to the file */
                List<string> objOutput;
                string objName;
                int objId;
                foreach (var geometry in m_Geometries)
                {
                    if (!geometry.GetGeometryName(out objName, out error))
                    {
                        streamWriter.Close();
                        return false;
                    }

                    if (!m_NameCounter.TryGetValue(objName, out objId)) /* new geometry type */
                    {
                        objId = 1;
                        m_NameCounter.Add(objName, objId);
                    }
                    else /* increase counter for this geometry type to ensure unique feature names  */
                    {
                        m_NameCounter[objName] = ++objId;
                    }

                    /* add the modified feature name  to the internam mapping  */
                    geometry.UpdateGeometryName(String.Format("{0}_{1}", objName, objId));
                    /* create the output string for this geometry */
                    if (!geometry.CreateOutput(objId, out objOutput, out error))
                    {
                        streamWriter.Close();
                        return false;
                    }
                    streamWriter.WriteLine(String.Join(",", objOutput));
                }

                streamWriter.Close();
            }
            catch (Exception ex)
            {
                error = "Error: Conversion failed due to: " + ex.Message;
                return false;
            }
            return true;
        }

        /**
         * Opens the configuration xml file. Extracts the input and output settings for each geometry type.
         */ 
        private bool ReadConfig(string configName, out string error)
        {

            error = "";
            string xmlValue = "";
            ;

            try
            {
                string xmlConfiguration = File.ReadAllText(configName);

                XmlDocument doc = new XmlDocument();
                XmlNode root = null;
                try
                {
                    doc.LoadXml(xmlConfiguration);
                    root = doc.SelectSingleNode("CSV_CONVERTER");

                    if (root == null || root.Name != "CSV_CONVERTER")
                    {
                        error = "Error: Xml file does not contain the node \"CSV_CONVERTER\"";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    error = "Error: Xml format error: " + ex.Message;
                    return false;
                }

                XmlHelper.XmlGetStringAttribute(doc, root, "", "delimiter", true, ";", out xmlValue, out error);
                m_Delimiter = xmlValue.ElementAt(0);

                /* iterate over all types */
                XmlNodeList type_Settings = root.SelectSingleNode("TYPE_SETTINGS").ChildNodes;
                string featurename;
                string typeKey;
                for (int typeIdx = 0; typeIdx < type_Settings.Count; typeIdx++)
                {

                    typeKey = type_Settings[typeIdx].Name;
                    /* check if input or output settings exist for this type */
                    if (m_InputSettings.ContainsKey(typeKey) || m_OutputSettings.ContainsKey(typeKey))
                    {
                        error = $"Error: Configuration settings of the type {typeKey} are not unique! Node exists multiple times!";
                        return false;
                    }

                    if (!XmlHelper.XmlGetStringAttribute(doc, type_Settings[typeIdx], "", "featurename", false, "", out featurename, out error))
                    {
                        return false;
                    }

                    /* get all input settings for this specific type */
                    if (!extractTypeInputSettings(doc, type_Settings[typeIdx], typeKey, out error))
                    {
                        return false;
                    }
                    /* get all output settings for this specific type */
                    if (!extractTypeOutputSettings(doc, type_Settings[typeIdx], typeKey, featurename, out error))
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                error = "Error: Configuration failed due to:" + ex.Message;
                return false;
            }
            return true;
        }

        /**
         * Add the geometry defined by the data string to the internal list of geometries.
         */ 
        private bool AddGeometry(string data, out string error)
        {

            error = "";

            List<string> args = data.Split(m_Delimiter).ToList();

            /* ignore empty lines */
            if (args.Count > 0)
            {
                BaseGeometry geometry;
                InputSettings inputSettings;
                OutputSettings outputSettings;

                switch (args.ElementAt(0))
                {
                    case "CIR":
                        {
                            geometry = new Circle();

                            if (!m_InputSettings.TryGetValue("CIR", out inputSettings))
                            {
                                error = "Error: Missing input configuration for CIR";
                                return false;
                            }
                            
                            if (!m_OutputSettings.TryGetValue("CIR", out outputSettings))
                            {
                                error = "Error: Missing input configuration for CIR";
                                return false;
                            }
                            break;
                        }
                    case "PLN":
                        {
                            geometry = new Plane();

                            if (!m_InputSettings.TryGetValue("PLN", out inputSettings))
                            {
                                error = "Error: Missing input configuration for PLN";
                                return false;
                            }

                            if (!m_OutputSettings.TryGetValue("PLN", out outputSettings))
                            {
                                error = "Error: Missing input configuration for PLN";
                                return false;
                            }
                            break;
                        }
                    case "CON":
                        {
                            geometry = new Cone();

                            if (!m_InputSettings.TryGetValue("CON", out inputSettings))
                            {
                                error = "Error: Missing input configuration for CON";
                                return false;
                            }

                            if (!m_OutputSettings.TryGetValue("CON", out outputSettings))
                            {
                                error = "Error: Missing input configuration for CON";
                                return false;
                            }
                            break;
                        }
                    default:
                        {
                            error = "Error: This type is not supported";
                            return false;
                        }
                }

                if (!geometry.CreateGeometry(args, inputSettings, outputSettings, out error))
                {
                    return false;
                }
                m_Geometries.Add(geometry);
            }
            return true;
        }

        /**
         * Extracts the input settings for a geometry type
         */ 
        private bool extractTypeInputSettings(XmlDocument doc, XmlNode settings, string typeKey, out string error)
        {
            error = "";
            int id;
            string xmlValue;

            XmlNodeList inputConfig = settings.SelectSingleNode("INPUT").ChildNodes;
            List<string> inputNames = Lists.Populate<string>(inputConfig.Count);
            List<string> inputTypes = Lists.Populate<string>(inputConfig.Count);
            /* iterate over all Child nodes of INPUT */
            for (int inputIdx = 0; inputIdx < inputConfig.Count; inputIdx++)
            {
                if (!XmlHelper.XmlGetStringAttribute(doc, inputConfig[inputIdx], "", "id", false, "", out xmlValue, out error))
                {
                    return false;
                }
                if (!Int32.TryParse(xmlValue, out id))
                {
                    error = $"Error: Unable to convert {xmlValue} to integer";
                    return false;
                }
                /* check if id is in its allowed range */
                if (id < 0 || id >= inputConfig.Count)
                {
                    error = $"Error: The id {id} is out of range";
                    return false;
                }

                if (!XmlHelper.XmlGetStringAttribute(doc, inputConfig[inputIdx], "", "name", false, "", out xmlValue, out error))
                {
                    return false;
                }
                inputNames[id] = xmlValue;
                if (!XmlHelper.XmlGetStringAttribute(doc, inputConfig[inputIdx], "", "type", true, "double", out xmlValue, out error))
                {
                    return false;
                }
                inputTypes[id] = xmlValue;
            }
            m_InputSettings.Add(typeKey, new InputSettings(inputNames, inputTypes));
            return true;
        }

        /**
         * Extract the output settings for a geometry type
         */ 
        private bool extractTypeOutputSettings(XmlDocument doc, XmlNode settings, string typeKey, string featurename, out string error)
        {
            error = "";
            string xmlValue;
            int id;
            uint precision;

            XmlNodeList outputConfig = settings.SelectSingleNode("OUTPUT").ChildNodes;

            List<string> outputNames = Lists.Populate<string>(outputConfig.Count);
            List<string> outputTypes = Lists.Populate<string>(outputConfig.Count);
            List<string> outputDefaults = Lists.Populate<string>(outputConfig.Count);
            List<uint> outputDecimals = Lists.Populate<uint>(outputConfig.Count);
            /* iterate over all Child Nodes of OUTPUT */
            for (int outputIdx = 0; outputIdx < outputConfig.Count; outputIdx++)
            {
                if (!XmlHelper.XmlGetStringAttribute(doc, outputConfig[outputIdx], "", "id", false, "", out xmlValue, out error))
                {
                    return false;
                }
                if (!Int32.TryParse(xmlValue, out id))
                {
                    error = $"Error: Unable to convert {xmlValue} to integer";
                    return false;
                }
                /* check if id is in its allowed range */
                if (id < 0 || id >= outputConfig.Count)
                {
                    error = $"Error: The id {id} is out of range";
                    return false;
                }

                if (!XmlHelper.XmlGetStringAttribute(doc, outputConfig[outputIdx], "", "name", false, "", out xmlValue, out error))
                {
                    return false;
                }
                outputNames[id] = xmlValue;

                if (!XmlHelper.XmlGetStringAttribute(doc, outputConfig[outputIdx], "", "type", true, "double", out xmlValue, out error))
                {
                    return false;
                }
                outputTypes[id] = xmlValue;

                if (!XmlHelper.XmlGetStringAttribute(doc, outputConfig[outputIdx], "", "default", true, "", out xmlValue, out error))
                {
                    return false;
                }
                outputDefaults[id] = xmlValue;

                if (!XmlHelper.XmlGetStringAttribute(doc, outputConfig[outputIdx], "", "decimaldigits", true, "0", out xmlValue, out error))
                {
                    return false;
                }

                if (!UInt32.TryParse(xmlValue, out precision))
                {
                    error = $"Error: Unable to convert {xmlValue} to uint";
                    return false;
                }
                outputDecimals[id] = precision;
            }
            m_OutputSettings.Add(typeKey, new OutputSettings(featurename, outputNames, outputTypes, outputDefaults, outputDecimals));
            return true;
        }

        /* members */
        private Dictionary<string, int> m_NameCounter;                    //!< the map containing the current name counter for each geometry type.
        private List<BaseGeometry> m_Geometries;                          //!< the List containing all geometries of the currently "opened and parsed" file.
        private Dictionary<string, InputSettings> m_InputSettings;        //!< the map containing the input settings for all specified geometry types.
        private Dictionary<string, OutputSettings> m_OutputSettings;      //!< the map containing the output settings for all specified geometry types.
        private char m_Delimiter;                                         //!< delimiter of the text file containing the input data.
    }
}
