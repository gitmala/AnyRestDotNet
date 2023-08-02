using System.Collections.Generic;

namespace AnyRest
{
    public class Endpoint
    {
        public string Id;
        public string RouteSpec;
        Dictionary<string, Action> VerbActions;

        public Endpoint(string id, string routeSpec, IEnumerable<KeyValuePair<string, Action>> verbActions)
        {
            Id = id;
            RouteSpec = routeSpec;
            VerbActions = new Dictionary<string, Action>(verbActions);
        }

        public Action GetAction(string verb)
        {
            return VerbActions[verb];
        }
    }

    public class Endpoints : List<Endpoint> { }
}