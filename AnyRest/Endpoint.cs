using System;
using System.Collections.Generic;

namespace AnyRest
{
    public class Endpoint
    {
        public string Id;
        public string BaseRoute;
        public string FullRoute;
        Dictionary<string, Action> VerbActions;

        public Endpoint(string id, string routePrefix, string route, IEnumerable<KeyValuePair<string, Action>> verbActions)
        {
            Id = id;

            if (id.Contains('{') || id.Contains('}'))
                throw new ArgumentException($"Id of endpoint {id} contains invalid charactars");
            if (routePrefix.Contains('{') || routePrefix.Contains('}'))
                throw new ArgumentException($"routePrefix of endpoint {id} contains invalid charactars");

            BaseRoute = $"/{routePrefix}/{id}";
            BaseRoute = BaseRoute.Replace("//", "/");

            FullRoute = $"/{BaseRoute}/{route}";
            FullRoute = FullRoute.Replace("//", "/").TrimEnd('/');

            VerbActions = new Dictionary<string, Action>(verbActions);
        }

        public Action GetAction(string verb)
        {
            return VerbActions[verb];
        }

        public string AsString()
        {
            return $"Id: {Id}, BaseRoute: {BaseRoute}, FullRoute: {FullRoute}";
        }
    }

    public class Endpoints : List<Endpoint>
    {
        Dictionary<string, Endpoint> usedBaseRoutes = new Dictionary<string, Endpoint>();
        
        public new void Add(Endpoint endpoint)
        {
            Endpoint existingEndpoint;
            if (usedBaseRoutes.TryGetValue(endpoint.BaseRoute, out existingEndpoint))
                throw new ArgumentException($"BaseRoute {endpoint.BaseRoute} is already used by endpoint {existingEndpoint.Id}");
            usedBaseRoutes.Add(endpoint.BaseRoute, endpoint);

            base.Add(endpoint);
        }
    }
}