using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.IO;

namespace AnyRest
{
    public class QueryparmDefaultsConfig
    {
        public string Type;
        public bool? Optional;

        [JsonConstructor]
        public QueryparmDefaultsConfig(string type, bool? optional)
        {
            Type = type == null ? QueryParm.DefaultType() : type;
            Optional = optional == null ? false : optional;
        }

        public QueryparmDefaultsConfig() : this(null, null) { }
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
            return new QueryParm(Name, type, (bool)optional);
        }
    }

    public class ActionDefaultsConfig
    {
        public string Type;
        public string Shell;
        public string ArgumentsPrefix;
        public string CommandLine;
        public string ContentType;
        public string DownloadFileName;

        [JsonConstructor]
        public ActionDefaultsConfig(string type, string shell, string argumentsPrefix, string commandLine, string contentType, string downloadFileName)
        {
            Type = type == null ? Action.DefaultType() : type;
            Shell = shell == null ? "" : shell;
            ArgumentsPrefix = argumentsPrefix == null ? "" : argumentsPrefix;
            CommandLine = commandLine == null ? "" : commandLine;
            ContentType = contentType == null ? "application/octet-stream" : contentType;
            DownloadFileName = downloadFileName;
        }

        public ActionDefaultsConfig() : this(null, null, null, null, null, null) { }
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

            return Action.Create(type, shell, argumentsPrefix, commandLine, queryParms, contentType, downloadFileName);
        }
    }

    public class EndpointDefaultsConfig
    {
        public string RoutePrefix;
        public string Route;

        [JsonConstructor]
        public EndpointDefaultsConfig(string routePrefix, string route)
        {
            RoutePrefix = routePrefix == null ? "" : routePrefix;
            Route = route == null ? "" : route;
        }

        public EndpointDefaultsConfig() : this(null, null) { }
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
            if (EndPointDefaults == null)
                EndPointDefaults = new EndpointDefaultsConfig();
            if (ActionDefaults == null)
                ActionDefaults = new ActionDefaultsConfig();
            if (QueryparmDefaults == null)
                QueryparmDefaults = new QueryparmDefaultsConfig();

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