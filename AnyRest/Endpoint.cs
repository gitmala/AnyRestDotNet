using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;

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

        private static Guid SetRequestId(HttpResponse response)
        {
            var requestId = Guid.NewGuid();
            response.Headers.Add("RequestId", requestId.ToString());
            return requestId;
        }

        public static IResult HandleDefaultRequest(HttpContext context, IRequestLogger logger)
        {
            var requestId = SetRequestId(context.Response);
            logger.LogRequest(context, requestId, "CatchAll", HttpStatusCode.NotFound, "No endpoint defined for route");
            return Results.NotFound("No endpoint defined for route");
        }

        public IResult HandleRequest(HttpContext context, IRequestLogger logger)
        {
            var requestId = SetRequestId(context.Response);
            string logExtraText = "";
            HttpStatusCode logStatusCode = HttpStatusCode.OK;

            IResult returnResult;

            try
            {
                var action = GetAction(context.Request.Method);
                try
                {
                    var actionEnvironment = action.MakeActionEnvironment(context.Request);
                    try
                    {
                        returnResult = action.Run(actionEnvironment, context.Response);
                    }
                    catch (Exception ex)
                    {
                        logStatusCode = HttpStatusCode.InternalServerError;
                        logExtraText = ex.Message;
                        returnResult = Results.Problem("Something bad happened handeling request");
                    }
                }
                catch (ApplicationException ex)
                {
                    logStatusCode = HttpStatusCode.BadRequest;
                    logExtraText = ex.Message;
                    returnResult = Results.BadRequest(logExtraText);
                }
            }
            catch (KeyNotFoundException)
            {
                logStatusCode = HttpStatusCode.NotFound;
                logExtraText = "Method not defined for endpoint";
                returnResult = Results.NotFound($"No method of type {context.Request.Method} defined for endpoint {Id}");
            }

            logger.LogRequest(context, requestId, Id, logStatusCode, logExtraText);
            return returnResult;
        }
    }

    public class Endpoints : List<Endpoint>
    {
        Dictionary<string, Endpoint> usedBaseRoutes = new();
        
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