using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AnyRest
{
    public class Service
    {
        public static void StartService(Endpoints endpoints)
        {
            var builder = WebHost.CreateDefaultBuilder()
                .ConfigureServices(x => x.AddSingleton<Endpoints>(endpoints))
                .ConfigureKestrel(options => options.Limits.MaxRequestBodySize = null)
                .ConfigureKestrel(options => options.ListenAnyIP(3354))
                .UseStartup<Startup>();

            builder.Build().Run();
        }
    }
}