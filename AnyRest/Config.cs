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
        public string DownloadFileName;
        public string ReturnOnOk;

        public Action AsAction()
        {
            switch (Type) {
                case "CommandResult":
                    return new CommandAction(CommandLine, Parms);
                case "Stream":
                    return new StreamAction(CommandLine, Parms, ContentType, DownloadFileName);
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

        public Endpoint AsEndpoint()
        {
            var actions = new List<KeyValuePair<string, Action>>();
            foreach (var Action in Actions)
                actions.Add(new KeyValuePair<string, Action>(Action.Method, Action.AsAction()));
            return new Endpoint(Id, Route, actions);
        }
    }

    public class FileConfig
    {
        public EndpointConfig[] Endpoints;
        public Endpoints AsEndpoints()
        {
            var endpoints = new Endpoints();
            foreach (var Endpoint in Endpoints)
                endpoints.Add(Endpoint.AsEndpoint());
            return endpoints;
        }
        public static Endpoints LoadFromFile(string fileName)
        {
            var gen = new Newtonsoft.Json.Schema.Generation.JSchemaGenerator();
            var schema = gen.Generate(typeof(FileConfig));
            return JsonConvert.DeserializeObject<FileConfig>(File.ReadAllText(fileName)).AsEndpoints();
        }
    }
}