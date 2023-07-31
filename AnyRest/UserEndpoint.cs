using System.Collections.Generic;

namespace AnyRest
{
    public class UserEndpoint
    {
        public string Id;
        public string RouteSpec;
        public Dictionary<string, ActionReturner> VerbActions;

        public UserEndpoint(string id, string routeSpec, IEnumerable<KeyValuePair<string, ActionReturner>> verbActions)
        {
            Id = id;
            RouteSpec = routeSpec;
            VerbActions = new Dictionary<string, ActionReturner>(verbActions);
        }

        public ActionReturner GetAction(string verb)
        {
            ActionReturner action;
            if (VerbActions.TryGetValue(verb, out action))
                return action;
            else
                return null;
        }
    }

    public class UserEndpoints : List<UserEndpoint> { }
}