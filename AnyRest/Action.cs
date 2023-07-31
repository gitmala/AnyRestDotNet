using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnyRest
{
    public abstract class ActionReturner
    {
        protected string CommandLine;
        protected string ContentType;
        protected string ContentDisposition = null;
        QueryParmConfig[] QueryParmConfig;

        public ActionReturner(string commandLine, string contentType, QueryParmConfig[] queryParmConfig, string contentDisposition)
        {
            CommandLine = commandLine;
            ContentType = contentType;
            QueryParmConfig = queryParmConfig;
            ContentDisposition = contentDisposition;
        }

        public abstract IActionResult ReturnFromCommand(HttpRequest request, HttpResponse response);
    }

    class CommandResultReturner : ActionReturner
    {
        public CommandResultReturner(string commandLine, QueryParmConfig[] queryParmConfig) : base(commandLine, null, queryParmConfig, null)
        {
        }
        public override IActionResult ReturnFromCommand(HttpRequest request, HttpResponse response)
        {
            var result = CommandExecuter.ExecuteCommand(CommandLine, request);
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

        public override IActionResult ReturnFromCommand(HttpRequest request, HttpResponse response)
        {
            var commandOutput = CommandExecuter.ExecuteDataCommand(CommandLine, request);
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