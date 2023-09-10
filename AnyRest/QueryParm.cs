using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AnyRest
{
    public class QueryParm
    {
        public string Name { get; set; }
        string Type;
        bool Optional;

        static readonly string[] QueryParmTypes = { "string", "int", "double", "bool" };
        public static string DefaultType() { return QueryParmTypes[0]; }

        public QueryParm(string name, string type, bool optional)
        {
            Name = name;
            if (!QueryParmTypes.Contains(type))
                throw new ArgumentException($"Invalid query parm type {type}");
            Type = type;
            Optional = optional;
        }

        static readonly IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
        public string GetValidated(string queryParm)
        {
            if (queryParm == null)
            {
                if (!Optional)
                    throw new ArgumentException($"Missing query parameter \"{Name}\"");
                return null;
            }
            else
            {
                try
                {
                    switch (Type)
                    {
                        case "int":
                            return int.Parse(queryParm).ToString(provider);
                        case "double":
                            return double.Parse(queryParm, NumberStyles.Float, provider).ToString(provider);
                        case "bool":
                            return bool.Parse(queryParm).ToString(provider);
                        default:
                            return queryParm;
                    }
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"\"{Name}\" is not a valid {Type}");
                }
            }
        }
    }

    public class QueryParmList : List<QueryParm> { }
}
