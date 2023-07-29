using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AnyRest
{
    public class QueryParmConfig
    {
        public string Name;
        public string Type;
        public bool Optional;
    }

    public class ActionConfig
    {
        public string Method;
        public string Type;
        public string CommandLine;
        public QueryParmConfig[] Parms;
        public string ContentType;
        public string ReturnOnOk;

        public IActionReturner AsAction()
        {
            switch (Type) {
                case "CommandResult":
                    return new CommandResultReturner(CommandLine);
                case "Stream":
                    return new FileStreamReturner(CommandLine, ContentType);
                default:
                    throw new ApplicationException("Unknown actiontype");
            }
        }
    }

    public class EndpointConfig
    {
        public string Id;
        public string Route;
        public ActionConfig[] Actions;

        public UserEndpoint AsEndpoint()
        {
            var actions = new List<KeyValuePair<string, IActionReturner>>();
            foreach (var Action in Actions)
                actions.Add(new KeyValuePair<string, IActionReturner>(Action.Method, Action.AsAction()));
            return new UserEndpoint(Id, Route, actions);
        }
    }

    public class FileConfig
    {
        public EndpointConfig[] Endpoints;
        public UserEndpoints AsEndpoints()
        {
            var endpoints = new UserEndpoints();
            foreach (var Endpoint in Endpoints)
                endpoints.Add(Endpoint.AsEndpoint());
            return endpoints;
        }
        public static UserEndpoints LoadFromFile(string fileName)
        {
            var gen = new Newtonsoft.Json.Schema.Generation.JSchemaGenerator();
            var schema = gen.Generate(typeof(FileConfig));
            return JsonConvert.DeserializeObject<FileConfig>(File.ReadAllText(fileName)).AsEndpoints();
        }
    }
}