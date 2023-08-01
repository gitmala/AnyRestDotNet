using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnyRest
{
    public class HttpEnvironment
    {
        public IEnumerable<KeyValuePair<string, string>> QueryParms = null;
        public List<KeyValuePair<string, string>> RouteValues = new List<KeyValuePair<string, string>>();
        public string RequestMethod = null;
        public string RequestPath = null;
        public string ContentType = null;
        public System.IO.Stream RequestBody = null;

        public HttpEnvironment(IEnumerable<KeyValuePair<string, string>> queryParms, string requestMethod, string requestPath, string contentType, System.IO.Stream requestBody)
        {
            QueryParms = queryParms;
            RequestMethod = requestMethod;
            RequestPath = requestPath;
            RequestBody = requestBody;
            ContentType = contentType;
        }
    }

    public abstract class ActionReturner
    {
        protected string CommandLine;
        public string ContentType;
        protected string ContentDisposition = null;
        QueryParmConfig[] QueryParmConfigs;

        public ActionReturner(string commandLine, string contentType, QueryParmConfig[] queryParmConfigs, string contentDisposition)
        {
            CommandLine = commandLine;
            if (string.IsNullOrEmpty(contentType))
                ContentType = "application/octet-stream";
            else
                ContentType = contentType;

            QueryParmConfigs = queryParmConfigs;
            ContentDisposition = contentDisposition;
        }

        public IEnumerable<KeyValuePair<string, string>> ValidateQueryParms(HttpRequest request)
        {
            var queryParms = new List<KeyValuePair<string, string>>();

            foreach (var queryParmConfig in QueryParmConfigs)
            {
                if (!request.Query.Keys.Contains(queryParmConfig.Name) && !queryParmConfig.Optional)
                {
                    throw new ApplicationException($"Missing query parameter {queryParmConfig.Name}");
                }
                else
                    queryParms.Add(new KeyValuePair<string, string>(queryParmConfig.Name, request.Query[queryParmConfig.Name]));
            }

            return queryParms;
        }

        public abstract IActionResult ReturnFromCommand(HttpEnvironment httpEnvironment, HttpResponse response);
    }

    class CommandResultReturner : ActionReturner
    {
        public CommandResultReturner(string commandLine, QueryParmConfig[] queryParmConfig) : base(commandLine, null, queryParmConfig, null)
        {
        }
        public override IActionResult ReturnFromCommand(HttpEnvironment httpEnvironment, HttpResponse response)
        {
            var result = CommandExecuter.ExecuteCommand(CommandLine, httpEnvironment);
            return new OkObjectResult(result);
        }
    }

    class FileStreamReturner : ActionReturner
    {
        public FileStreamReturner(string commandLine, QueryParmConfig[] queryParmConfig, string contentType) : base(commandLine, contentType, queryParmConfig, null)
        {
        }

        public FileStreamReturner(string commandLine, string contentType, QueryParmConfig[] queryParmConfig, string contentDisposition) : base(commandLine, contentType, queryParmConfig, contentDisposition)
        {
        }

        public override IActionResult ReturnFromCommand(HttpEnvironment httpEnvironment, HttpResponse response)
        {
            var commandOutput = CommandExecuter.ExecuteDataCommand(CommandLine, httpEnvironment);
            if (ContentDisposition != null)
                response.Headers.Add("Content-Disposition", ContentDisposition);
            return new FileStreamResult(commandOutput, ContentType);
        }
    }

    class Action
    {
        public string HttpMethod;
        public string CommandFile;
        public string CommandArguments;

        public ActionReturner actionReturner;
    }

}