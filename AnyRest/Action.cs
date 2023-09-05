using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ActionParm = System.Collections.Generic.KeyValuePair<string, string>;

namespace AnyRest
{
    public class QueryParm
    {
        static readonly string[] QueryParmTypes = { "string", "int", "double", "bool" };
        public static string DefaultType() { return QueryParmTypes[0]; }

        public string Name;
        public string Type;
        public bool Optional;

        public QueryParm(string name, string type, bool optional)
        {
            Name = name;
            if (!QueryParmTypes.Contains(type))
                throw new ArgumentException($"Invalid query parm type {type}");
            Type = type;
            Optional = optional;
        }

        static readonly IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
        public string CheckType(string queryParm)
        {
            switch (Type)
            {
                case "int":
                    return int.Parse(queryParm).ToString(provider);
                case "double":
                    return double.Parse(queryParm, NumberStyles.Float, provider).ToString(provider);
                case "bool":
                    return bool.Parse(queryParm).ToString(provider);
                default:
                    return queryParm;
            }
        }
    }

    public class QueryParms : List<QueryParm> { }

    public class ActionParms : List<ActionParm> { }

    public class ActionEnvironment
    {
        public ActionParms QueryParms = new();
        public ActionParms RouteValues = new();
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

    public abstract class Action
    {
        static readonly string[] ActionTypes = { "stream" , "command" };
        public static string DefaultType() { return ActionTypes[0]; }

        protected string Shell;
        protected string ArgumentsPrefix;
        protected string Arguments;
        QueryParms queryParms;
        protected string ContentType;

        protected Action(string shell, string argumentsPrefix, string arguments, QueryParms queryParms, string contentType)
        {
            Shell = shell;
            ArgumentsPrefix = argumentsPrefix;
            Arguments = arguments;
            this.queryParms = queryParms;
            ContentType = contentType;
        }

        public static Action Create(string type, string shell, string argumentsPrefix, string arguments, QueryParms queryParms, string contentType, string downloadFileName)
        {
            if (type == ActionTypes[0])
                return new StreamAction(shell, argumentsPrefix, arguments, queryParms, contentType, downloadFileName);
            else if (type == ActionTypes[1])
                return new CommandAction(shell, argumentsPrefix, arguments, queryParms);
            else
                throw new ApplicationException($"Unknown actiontype \"{type}\"");
        }

        public ActionEnvironment MakeActionEnvironment(HttpRequest request, Guid requestId)
        {
            var actionEnvironment = new ActionEnvironment(request.Method, request.Path, ContentType, requestId, request.Body);
            var parsedQueryParms = QueryHelpers.ParseQuery(request.QueryString.Value);
            foreach (var parsedQueryParm in parsedQueryParms)
            {
                if (parsedQueryParm.Value.Count > 1)
                    throw new ArgumentException($"Duplicate query parameter not allowed (parameter \"{parsedQueryParm.Key}\")");
            }

            foreach (var queryParm in queryParms)
            {
                string queryParmStringValue = request.Query[queryParm.Name];
                if (queryParmStringValue == null)
                {
                    if (!queryParm.Optional)
                        throw new ArgumentException($"Missing query parameter \"{queryParm.Name}\"");
                }
                else
                {
                    string stringValue;
                    try
                    {
                        stringValue = queryParm.CheckType(request.Query[queryParm.Name]);
                    }
                    catch
                    {
                        throw new ArgumentException($"\"{queryParm.Name}\" is not a valid {queryParm.Type}");
                    }

                    actionEnvironment.AddQueryParm(queryParm.Name, stringValue);
                }
            }

            foreach (var routeValue in request.RouteValues)
            {
                if (routeValue.Value != null && routeValue.Value.GetType() == typeof(string))
                    actionEnvironment.AddRouteParm(routeValue.Key, (string)routeValue.Value);
            }

            return actionEnvironment;
        }

        public abstract IResult Run(ActionEnvironment actionEnvironment, HttpResponse response);
    }

    class CommandAction : Action
    {
        public CommandAction(string shell, string argumentsPrefix, string arguments, QueryParms queryParms) : base(shell, argumentsPrefix, arguments, queryParms, null)
        {
        }
        public override IResult Run(ActionEnvironment actionEnvironment, HttpResponse response)
        {
            var result = ShellExecuter.GetCommandResult(Shell, ArgumentsPrefix, Arguments, actionEnvironment);
            return Results.Ok(result);
        }
    }

    class StreamAction : Action
    {
        protected string DownloadFileName = null;
        public StreamAction(string shell, string argumentsPrefix, string arguments, QueryParms queryParms, string contentType, string downloadFileName) : base(shell, argumentsPrefix, arguments, queryParms, contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                ContentType = "application/octet-stream";
            else
                ContentType = contentType;
            DownloadFileName = downloadFileName;
        }

        public override IResult Run(ActionEnvironment actionEnvironment, HttpResponse response)
        {
            var commandOutput = ShellExecuter.GetStreamResult(Shell, ArgumentsPrefix, Arguments, actionEnvironment);
            if (!string.IsNullOrEmpty(DownloadFileName))
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{DownloadFileName}\"");
            else
                response.Headers.Add("Content-Disposition", "inline");
            return Results.Stream(commandOutput, ContentType);
        }
    }

}