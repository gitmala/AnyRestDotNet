﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace AnyRest
{
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
        protected string CommandLine;
        public string ContentType;
        protected string ContentDisposition = null;
        QueryParmConfig[] QueryParmConfigs;

        protected Action(string commandLine, string contentType, QueryParmConfig[] queryParmSpec, string contentDisposition)
        {
            CommandLine = commandLine;
            if (string.IsNullOrEmpty(contentType))
                ContentType = "application/octet-stream";
            else
                ContentType = contentType;

            QueryParmConfigs = queryParmSpec;
            ContentDisposition = contentDisposition;
        }

        public ActionEnvironment MakeActionEnvironment(HttpRequest request)
        {
            var actionEnvironment = new ActionEnvironment(request.Method, request.Path, ContentType, request.Body);

            foreach (var queryParmConfig in QueryParmConfigs)
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

        public abstract IActionResult Run(ActionEnvironment httpEnvironment, HttpResponse response);
    }

    class CommandAction : Action
    {
        public CommandAction(string commandLine, QueryParmConfig[] queryParmConfig) : base(commandLine, null, queryParmConfig, null)
        {
        }
        public override IActionResult Run(ActionEnvironment httpEnvironment, HttpResponse response)
        {
            var result = ShellExecuter.GetCommandResult(CommandLine, httpEnvironment);
            return new OkObjectResult(result);
        }
    }

    class StreamAction : Action
    {
        public StreamAction(string commandLine, QueryParmConfig[] queryParmConfig, string contentType, string contentDisposition) : base(commandLine, contentType, queryParmConfig, contentDisposition)
        {
        }

        public override IActionResult Run(ActionEnvironment httpEnvironment, HttpResponse response)
        {
            var commandOutput = ShellExecuter.GetStreamResult(CommandLine, httpEnvironment);
            if (ContentDisposition != null)
                response.Headers.Add("Content-Disposition", ContentDisposition);
            return new FileStreamResult(commandOutput, ContentType);
        }
    }

}