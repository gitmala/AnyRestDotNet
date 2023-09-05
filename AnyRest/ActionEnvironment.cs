using System;
using System.Collections.Generic;

using ActionParm = System.Collections.Generic.KeyValuePair<string, string>;

namespace AnyRest
{
    public class ActionParmList : List<ActionParm> { }

    public class ActionEnvironment
    {
        public ActionParmList QueryParms = new();
        public ActionParmList RouteValues = new();
        public string RequestMethod = null;
        public string RequestPath = null;
        public string RequestId = null;
        public string ContentType = null;
        public System.IO.Stream RequestBody = null;

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
