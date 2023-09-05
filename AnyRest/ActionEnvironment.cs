using System;
using System.Collections.Generic;

using ActionParm = System.Collections.Generic.KeyValuePair<string, string>;

namespace AnyRest
{
    public class ActionParmList : List<ActionParm> { }

    public class ActionEnvironment
    {
        public ActionParmList QueryParms { get; set; } = new();
        public ActionParmList RouteValues { get; set; } = new();
        public string RequestMethod { get; set; }
        public string RequestPath { get; set; }
        public string RequestId { get; set; }
        public string ContentType { get; set; }
        public System.IO.Stream RequestBody { get; set; }

        public ActionEnvironment(string requestMethod, string requestPath, string contentType, Guid requestId, System.IO.Stream requestBody)
        {
            RequestMethod = requestMethod;
            RequestPath = requestPath;
            RequestBody = requestBody;
            ContentType = contentType;
            RequestId = requestId.ToString();
        }

        public void AddQueryParm(string name, string value)
        {
            QueryParms.Add(new ActionParm(name, value));
        }

        public void AddRouteParm(string name, string value)
        {
            RouteValues.Add(new ActionParm(name, value));
        }

    }
}
