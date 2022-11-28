using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace AnyRest
{
    public class Service
    {
        public static void StartService()
        {
            /*
                        var endpointSpecifications = new EndpointSpecifications();

                        endpointSpecifications.Add(new EndpointSpecification("A", "api1/foo",
                            new KeyValuePair<string, IAction>[] {
                                new KeyValuePair<string, IAction>("GET", new CommandResultAction("type %AnyRESTQueryParm_file%"))
                            }
                        ));

                        endpointSpecifications.Add(new EndpointSpecification("A", "api2/foo/{file}",
                            new List<KeyValuePair<string, IAction>> {
                                new KeyValuePair<string, IAction>("GET", new StreamAction("type %AnyRESTRouteParm_file%", "image/jpeg"))
                            }
                        ));

                        endpointSpecifications.Add(new EndpointSpecification("B", "api1/bar",
                            new List<KeyValuePair<string, IAction>> {
                                new KeyValuePair<string, IAction>("POST", new StreamAction(".\\AnyRest.exe -i", "image/jpeg"))
                            }
                        ));

                        endpointSpecifications.Add(new EndpointSpecification("B", "api3/bar",
                            new List<KeyValuePair<string, IAction>> {
                                new KeyValuePair<string, IAction>("GET", new CommandResultAction("echo hej")),
                                new KeyValuePair<string, IAction>("PUT", new CommandResultAction("echo hej"))
                            }
                        ));
            */
            var userEndpoints = FileConfig.LoadFromFile("config.json");
            var builder = WebHost.CreateDefaultBuilder()
                .ConfigureServices(x => x.AddSingleton<UserEndpoints>(userEndpoints))
                .ConfigureKestrel(options => options.Limits.MaxRequestBodySize = null)
                .ConfigureKestrel(options => options.ListenAnyIP(3354))
                .UseStartup<Startup>();

            builder.Build().Run();
        }
    }
}