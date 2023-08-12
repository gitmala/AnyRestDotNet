using System.Collections.Generic;

namespace AnyRest
{
    public class Endpoint
    {
        public string Id;
        public string RouteSpec;
        Dictionary<string, Action> VerbActions;

        public Endpoint(string id, string routePrefix, string route, IEnumerable<KeyValuePair<string, Action>> verbActions)
        {
            Id = id;
            RouteSpec = route;
            VerbActions = new Dictionary<string, Action>(verbActions);
        }

        public Action GetAction(string verb)
        {
            return VerbActions[verb];
        }

        public string AsString()
        {
            return Id + ": " + RouteSpec;
        }
    }

    public class Endpoints : List<Endpoint> { }
}