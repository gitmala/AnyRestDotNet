using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace AnyRest
{
    public class QueryparmDefaultsConfig
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("string")]
        public string Type;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool Optional;
    }

    public class QueryParmConfig
    {
        public string Name;   //No default since required
        public string Type;   //Has default
        public bool Optional; //Has default
    }

    public class ActionDefaultsConfig
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("stream")]
        public string Type;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Shell;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string ArgumentsPrefix;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string CommandLine;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("application/octet-stream")]
        public string ContentType;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(null)]
        public string DownloadFileName;
    }

    public class ActionConfig
    {
        public string Method;            //No default since required
        public string Type;              //Has default
        public string Shell;             //Has default
        public string ArgumentsPrefix;   //Has default
        public string CommandLine;       //Has default
        public QueryParmConfig[] Parms;  //No default, must be created if null
        public string ContentType;       //Has default
        public string DownloadFileName;  //Has default, but no defaultDefault. Must be handled by StreamAction constructor

        public Action AsAction()
        {
            QueryParmConfig[] parms = null;
            if (Parms == null)
                parms = new QueryParmConfig[0];
            else
                parms = Parms;

            switch (Type.ToLower()) {
                case "command":
                    return new CommandAction(CommandLine, parms);
                case "stream":
                    return new StreamAction(CommandLine, parms, ContentType, DownloadFileName);
                default:
                    throw new ApplicationException($"Unknown actiontype \"{Type}\"");
            }
        }
    }

    public class EndpointDefaultsConfig
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string RoutePrefix;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Route;
    }

    public class EndpointConfig
    {
        public string Id;            //No default since required
        public string RoutePrefix;   //Has default
        public string Route;         //Has default

        public ActionConfig[] Actions;

        public Endpoint AsEndpoint()
        {
            var actions = new List<KeyValuePair<string, Action>>();
            if (Actions != null)
            {
                foreach (var Action in Actions)
                    actions.Add(new KeyValuePair<string, Action>(Action.Method, Action.AsAction()));
            }
            return new Endpoint(Id, Route, actions);
        }
    }

    public class FileConfig
    {
        public QueryparmDefaultsConfig QueryparmDefaults;
        public ActionDefaultsConfig ActionDefaults;
        public EndpointDefaultsConfig EndPointDefaults;

        public EndpointConfig[] Endpoints;
        public Endpoints AsEndpoints()
        {
            var endpoints = new Endpoints();
            if (Endpoints != null)
            {
                foreach (var Endpoint in Endpoints)
                    endpoints.Add(Endpoint.AsEndpoint());
            }
            return endpoints;
        }

        public static Endpoints LoadFromFile(string fileName)
        {
            var configSchema = File.ReadAllText("configSchema.json");
            var config = File.ReadAllText(fileName);

            var reader = new JsonTextReader(new StringReader(config));
            var validatingReader = new JSchemaValidatingReader(reader);
            validatingReader.Schema = JSchema.Parse(configSchema);

            var serializer = new JsonSerializer();
            var fileConfig = serializer.Deserialize<FileConfig>(validatingReader);

            return fileConfig.AsEndpoints();
        }
    }
}