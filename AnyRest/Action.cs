﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyRest
{
    public class QueryParm
    {
        public static readonly string[] QueryParmTypes = { "string", "int", "double", "bool" };
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
    }

    public class QueryParms : List<QueryParm> { }

    public class ActionEnvironment
    {
        public List<KeyValuePair<string, string>> QueryParms = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> RouteValues = new List<KeyValuePair<string, string>>();
        public string RequestMethod = null;
        public string RequestPath = null;
        public string ContentType = null;
        public System.IO.Stream RequestBody = null;

        public ActionEnvironment(string requestMethod, string requestPath, string contentType, System.IO.Stream requestBody)
        {
            RequestMethod = requestMethod;
            RequestPath = requestPath;
            RequestBody = requestBody;
            ContentType = contentType;
        }

        public void AddQueryParm(string name, string value)
        {
            QueryParms.Add(new KeyValuePair<string, string>(name, value));
        }

        public void AddRouteParm(string name, string value)
        {
            RouteValues.Add(new KeyValuePair<string, string>(name, value));
        }

    }

    public abstract class Action
    {
        public static readonly string[] ActionTypes = { "stream" , "command" };
        public static string DefaultType() { return ActionTypes[0]; }

        public string Shell;
        public string ArgumentsPrefix;
        protected string CommandLine;
        QueryParms queryParms;
        protected string ContentType;

        protected Action(string shell, string argumentsPrefix, string commandLine, QueryParms queryParms, string contentType)
        {
            Shell = shell;
            ArgumentsPrefix = argumentsPrefix;
            CommandLine = commandLine;
            this.queryParms = queryParms;
            ContentType = contentType;
        }

        public static Action Create(string type, string shell, string argumentsPrefix, string commandLine, QueryParms queryParms, string contentType, string downloadFileName)
        {
            if (type == ActionTypes[0])
                return new StreamAction(shell, argumentsPrefix, commandLine, queryParms, contentType, downloadFileName);
            else if (type == ActionTypes[1])
                return new CommandAction(shell, argumentsPrefix, commandLine, queryParms);
            else
                throw new ApplicationException($"Unknown actiontype \"{type}\"");
        }

        public ActionEnvironment MakeActionEnvironment(HttpRequest request)
        {
            var actionEnvironment = new ActionEnvironment(request.Method, request.Path, ContentType, request.Body);

            foreach (var queryParmConfig in queryParms)
            {
                if (!request.Query.Keys.Contains(queryParmConfig.Name) && !queryParmConfig.Optional)
                    throw new ApplicationException($"Missing query parameter {queryParmConfig.Name}");
                else
                    actionEnvironment.AddQueryParm(queryParmConfig.Name, request.Query[queryParmConfig.Name]);
            }

            foreach (var routeValue in request.RouteValues)
            {
                if (routeValue.Value != null && routeValue.Value.GetType() == typeof(string))
                    actionEnvironment.AddRouteParm(routeValue.Key, (string)routeValue.Value);
            }

            return actionEnvironment;
        }

        public abstract IActionResult Run(ActionEnvironment actionEnvironment, HttpResponse response);
    }

    class CommandAction : Action
    {
        public CommandAction(string shell, string argumentsPrefix, string commandLine, QueryParms queryParms) : base(shell, argumentsPrefix, commandLine, queryParms, null)
        {
        }
        public override IActionResult Run(ActionEnvironment actionEnvironment, HttpResponse response)
        {
            var result = ShellExecuter.GetCommandResult(CommandLine, actionEnvironment);
            return new OkObjectResult(result);
        }
    }

    class StreamAction : Action
    {
        protected string DownloadFileName = null;
        public StreamAction(string shell, string argumentsPrefix, string commandLine, QueryParms queryParms, string contentType, string downloadFileName) : base(shell, argumentsPrefix, commandLine, queryParms, contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                ContentType = "application/octet-stream";
            else
                ContentType = contentType;
            DownloadFileName = downloadFileName;
        }

        public override IActionResult Run(ActionEnvironment actionEnvironment, HttpResponse response)
        {
            var commandOutput = ShellExecuter.GetStreamResult(CommandLine, actionEnvironment);
            if (DownloadFileName != null)
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{DownloadFileName}\"");
            else
                response.Headers.Add("Content-Disposition", "inline");
            return new FileStreamResult(commandOutput, ContentType);
        }
    }

}