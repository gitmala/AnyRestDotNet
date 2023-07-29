using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnyRest
{
    public interface IActionReturner
    {
        IActionResult ReturnFromCommand(HttpRequest request, HttpResponse response);
    }

    struct CommandResultReturner : IActionReturner
    {
        string Command;
        public CommandResultReturner(string command)
        {
            Command = command;
        }
        public IActionResult ReturnFromCommand(HttpRequest request, HttpResponse response)
        {
            var result = CommandExecuter.ExecuteCommand(Command, request);
            return new OkObjectResult(result);
        }
    }

    class FileStreamReturner : IActionReturner
    {
        string CommandLine;
        string ContentType;
        string ContentDisposition = null;
        public FileStreamReturner(string commandLine, string contentType)
        {
            CommandLine = commandLine;
            ContentType = contentType;
        }

        public FileStreamReturner(string commandLine, string contentType, string contentDisposition) : this(commandLine, contentType)
        {
            ContentDisposition = contentDisposition;
        }

        public IActionResult ReturnFromCommand(HttpRequest request, HttpResponse response)
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

        public IActionReturner actionReturner;
    }

}