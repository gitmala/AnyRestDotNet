﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;

namespace AnyRest
{
    public abstract class Action
    {
        static readonly string[] ActionTypes = { "stream" , "command" };
        public static string DefaultType() { return ActionTypes[0]; }

        protected string Shell;
        protected string ArgumentsPrefix;
        protected string Arguments;
        QueryParmList QueryParms;
        protected string ContentType;

        protected Action(string shell, string argumentsPrefix, string arguments, QueryParmList queryParms, string contentType)
        {
            Shell = shell;
            ArgumentsPrefix = argumentsPrefix;
            Arguments = arguments;
            QueryParms = queryParms;
            ContentType = contentType;
        }

        public static Action Create(string type, string shell, string argumentsPrefix, string arguments, QueryParmList queryParms, string contentType, string downloadFileName)
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
            var parsedQueryParms = QueryHelpers.ParseQuery(request.QueryString.Value);
            foreach (var parsedQueryParm in parsedQueryParms)
            {
                if (parsedQueryParm.Value.Count > 1)
                    throw new ArgumentException($"Duplicate query parameter not allowed (parameter \"{parsedQueryParm.Key}\")");
            }

            var actionEnvironment = new ActionEnvironment(request.Method, request.Path, ContentType, requestId, request.Body);

            foreach (var queryParm in QueryParms)
            {
                var stringValue = queryParm.GetValidated(request.Query[queryParm.Name]);
                if (stringValue != null)
                    actionEnvironment.AddQueryParm(queryParm.Name, stringValue);
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
        public CommandAction(string shell, string argumentsPrefix, string arguments, QueryParmList queryParms) : base(shell, argumentsPrefix, arguments, queryParms, null)
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
        string DownloadFileName;
        public StreamAction(string shell, string argumentsPrefix, string arguments, QueryParmList queryParms, string contentType, string downloadFileName) : base(shell, argumentsPrefix, arguments, queryParms, contentType)
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