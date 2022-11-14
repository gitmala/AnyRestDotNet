using System.Collections.Generic;

namespace AnyRest
{
    public class UserEndpoint
    {
        public string Id;
        public string RouteSpec;
        public Dictionary<string, IActionReturner> VerbActions;

        public UserEndpoint(string id, string routeSpec, IEnumerable<KeyValuePair<string, IActionReturner>> verbActions)
        {
            Id = id;
            RouteSpec = routeSpec;
            VerbActions = new Dictionary<string, IActionReturner>(verbActions);
        }

        public IActionReturner GetAction(string verb)
        {
            IActionReturner action;
            if (VerbActions.TryGetValue(verb, out action))
                return action;
            else
                return null;
        }
    }

    public class UserEndpoints : List<UserEndpoint> { }
}