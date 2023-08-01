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