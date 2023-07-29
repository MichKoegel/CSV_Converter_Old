using System.Collections.Generic;

namespace CSV_Converter
{
    /**
     * helper struct containing the configurable input settings
     */ 
    struct InputSettings
    {
        public List<string> Names { get; private set; }              //!< the keys for the internal mapping of the input data 
        public List<string> Types { get; private set; }              //!< the types of the input data

        /**
         * Constructor
         */ 
        public InputSettings(List<string> name, List<string> type)
        {
            Names = name;
            Types = type;
        }
    }

    /**
     * helper struct containing the configurable output settings
     */
    struct OutputSettings
    {
        public string FeatureName { get; private set; }              //!< the feature name of the geometry
        public List<string> Names { get; private set; }              //!< the keys for the output parameter
        public List<string> Types { get; private set; }              //!< the types of the output parameter
        public List<string> DefVals { get; private set; }            //!< the default values of the output parameter
        public List<uint> DecimalDigits { get; private set; }        //!< the number of decimal digits of the output parameter. Only relevant for numeric types

        /**
         * Constructor
         */
        public OutputSettings(string feature, List<string> name, List<string> type, List<string> defaults, List<uint> decimals)
        {
            FeatureName = feature;
            Names = name;
            Types = type;
            DefVals = defaults;
            DecimalDigits = decimals;
        }
    }
}
