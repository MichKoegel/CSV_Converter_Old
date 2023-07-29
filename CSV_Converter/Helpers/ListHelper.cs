using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV_Converter
{
    /** 
     * helper functions for the initialisation and pre-population of Lists of a
     * known size
     */
    public static class Lists
    {
        public static List<T> Populate<T>(int capacity)
        {
            return Repeated(default(T), capacity);
        }

        public static List<T> Repeated<T>(T value, int capacity)
        {
            List<T> list = new List<T>(capacity);
            list.AddRange(Enumerable.Repeat(value, capacity));
            return list;
        }


    }


}
