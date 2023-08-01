using System.Collections.Generic;

namespace AnyRest
{
    public class Endpoint
    {
        public string Id;
        public string RouteSpec;
        public Dictionary<string, Action> VerbActions;

        public Endpoint(string id, string routeSpec, IEnumerable<KeyValuePair<string, Action>> verbActions)
        {
            Id = id;
            RouteSpec = routeSpec;
            VerbActions = new Dictionary<string, Action>(verbActions);
        }

        public Action GetAction(string verb)
        {
            Action action;
            if (VerbActions.TryGetValue(verb, out action))
                return action;
            else
                return null;
        }
    }

    public class Endpoints : List<Endpoint> { }
}