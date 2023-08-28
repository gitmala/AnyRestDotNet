using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace AnyRest
{
    public class Endpoint
    {
        public string Id;
        public string BaseRoute;
        public string FullRoute;
        Dictionary<string, Action> VerbActions;

        const string InvalidChars = "{}*:";
        static readonly Regex IdInvalid = new($"[{InvalidChars + '/'}]");
        static readonly Regex RoutePrefixInvalid = new($"[{InvalidChars}]");

        public Endpoint(string id, string routePrefix, string route, IEnumerable<KeyValuePair<string, Action>> verbActions)
        {
            if (id == "")
                throw new ArgumentException($"Id of endpoint cannot be \"\"");
            if (id.ToLower() == CatchAllId)
                throw new ArgumentException($"Id of endpoint cannot be \"{CatchAllId}\" (Reserved word)");
            if (id.ToLower() == "builtin")
                throw new ArgumentException($"Id of endpoint cannot be \"builtin\" (Reserved word)");
            if (IdInvalid.IsMatch(id))
                throw new ArgumentException($"Id of endpoint {id} contains invalid charactars");
            if (RoutePrefixInvalid.IsMatch(routePrefix))
                throw new ArgumentException($"routePrefix of endpoint {id} contains invalid charactars");

            Id = id;

            BaseRoute = $"{routePrefix.Trim('/')}/{id}";

            FullRoute = $"/{BaseRoute}/{route.Trim('/')}".TrimEnd('/');

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

        const string CatchAllId = "fallback";
        public static IResult HandleFallBackRequest(HttpContext context, IRequestLogger logger)
        {
            var requestId = SetRequestId(context.Response);
            logger.LogRequest(context, requestId, CatchAllId, HttpStatusCode.NotFound, "No endpoint defined for route");
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