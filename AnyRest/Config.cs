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
        public bool? Optional; //Has default

        public QueryParm AsQueryParm(QueryparmDefaultsConfig queryparmDefaultsConfig)
        {
            var type = Type != null ? Type : queryparmDefaultsConfig.Type;
            var optional = Optional != null ? (bool)Optional : queryparmDefaultsConfig.Optional;
            return new QueryParm(Name, type, optional);
        }
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
        public QueryParmConfig[] Parms;  //No default
        public string ContentType;       //Has default
        public string DownloadFileName;  //Has default, but no defaultDefault. Must be handled by StreamAction constructor

        public Action AsAction(ActionDefaultsConfig actionDefaults, QueryparmDefaultsConfig queryparmDefaults)
        {
            var queryParms = new QueryParms();
            if (Parms != null)
            {
                foreach (var parm in Parms)
                {
                    queryParms.Add(parm.AsQueryParm(queryparmDefaults));
                }
            }

            var type = Type != null ? Type : actionDefaults.Type;
            var shell = Shell != null ? Shell : actionDefaults.Shell;
            var argumentsPrefix = ArgumentsPrefix != null ? ArgumentsPrefix : actionDefaults.ArgumentsPrefix;
            var commandLine = CommandLine != null ? CommandLine : actionDefaults.CommandLine;
            var contentType = ContentType != null ? ContentType : actionDefaults.ContentType;
            var downloadFileName = DownloadFileName != null ? DownloadFileName : actionDefaults.DownloadFileName;

            switch (type) {
                case "command":
                    return new CommandAction(shell, argumentsPrefix, commandLine, queryParms);
                case "stream":
                    return new StreamAction(shell, argumentsPrefix, commandLine, queryParms, contentType, downloadFileName);
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

        public Endpoint AsEndpoint(EndpointDefaultsConfig endPointDefaults, ActionDefaultsConfig actionDefaults, QueryparmDefaultsConfig queryparmDefaults)
        {
            var routePrefix = RoutePrefix != null ? RoutePrefix : endPointDefaults.RoutePrefix;
            var route = Route != null ? Route : endPointDefaults.Route;

            var actions = new List<KeyValuePair<string, Action>>();
            if (Actions != null)
            {
                foreach (var Action in Actions)
                    actions.Add(new KeyValuePair<string, Action>(Action.Method, Action.AsAction(actionDefaults, queryparmDefaults)));
            }
            return new Endpoint(Id, routePrefix, route, actions);
        }
    }

    public class FileConfig
    {
        public EndpointDefaultsConfig EndPointDefaults;
        public ActionDefaultsConfig ActionDefaults;
        public QueryparmDefaultsConfig QueryparmDefaults;

        public EndpointConfig[] Endpoints;
        public Endpoints AsEndpoints()
        {
            var endpoints = new Endpoints();
            if (Endpoints != null)
            {
                foreach (var Endpoint in Endpoints)
                    endpoints.Add(Endpoint.AsEndpoint(EndPointDefaults, ActionDefaults, QueryparmDefaults));
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