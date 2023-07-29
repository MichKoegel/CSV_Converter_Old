using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CSV_Converter
{
    class XmlHelper
    {
        /**
         * Helper function for extracting the string value of an attribute or using a default value if the attribute does not exist
         */ 
        public static bool XmlGetStringAttribute(XmlDocument doc, XmlNode xmlRoot, string path, string attributeName, bool createIfNotExist, string defaultValue, out string value, out string error)
        {
            value = "";
            error = "";
            XmlNode xmlNode;

            if (String.IsNullOrWhiteSpace(path))
            {
                xmlNode = xmlRoot;
            }
            else
            {
                xmlNode = xmlRoot.SelectSingleNode(path);
            }

            if (xmlNode == null)
            {
                error = string.Format("Error: Could not find attribute \"{0}\" in path \"{1}\": node does not exist", attributeName, path);
                return false;

            }
            if (xmlNode.Attributes[attributeName] == null)
            {
                if (createIfNotExist == false)
                {
                    error = string.Format("Error: Could not find attribute \"{0}\" in path \"{1}\": attribute does not exist", attributeName, path);
                    return false;
                }
                else
                {
                    XmlAttribute xmlAttr = doc.CreateAttribute(attributeName);
                    xmlAttr.Value = defaultValue;
                    xmlNode.Attributes.Append(xmlAttr);
                }
            }
            value = xmlNode.Attributes[attributeName].Value;
            return true;
        }


    }
}
