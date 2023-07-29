using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting;
using System.Threading.Tasks;

namespace AnyRest
{
    public class TranslationTransformer : DynamicRouteValueTransformer
    {
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            //if (values["trailing_data"] == null)
            {
                values["controller"] = "DynamicEndpoint";
                values["action"] = "MethodHandler";
                //values["action"] = httpContext.Request.Method;
                values["endpointSpecification"] = State;
            }
            return values;
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddXmlSerializerFormatters();

            services.AddLogging(c => c.ClearProviders());

            //services.AddTransient<TranslationTransformer>();

            services.AddSwaggerGen(/*c =>
            {
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = $"Description",
                    Type = SecuritySchemeType.ApiKey,
                    Name = "apikey",
                    In = ParameterLocation.Header,
                    Scheme = "ApiKeyScheme"
                });
                var key = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    },
                    In = ParameterLocation.Header
                };
                var requirement = new OpenApiSecurityRequirement { { key, new List<string>() } };

                c.AddSecurityRequirement(requirement);
            }*/);

            services.AddSwaggerGenWithConventionalRoutes(options =>
            {
            //options.IgnoreTemplateFunc = (template) => template.StartsWith("api/");
            //options.SkipDefaults = true;
        });
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "test");
            });

            var userEndpoints = (UserEndpoints)app.ApplicationServices.GetService(typeof(UserEndpoints));

            app.UseEndpoints(endpoints =>
            {
                foreach (UserEndpoint endpointSpecification in userEndpoints)
                {
                    //endpoints.MapDynamicControllerRoute<TranslationTransformer>(endpointSpecification.URL, endpointSpecification);
                    endpoints.MapControllerRoute(endpointSpecification.Id, endpointSpecification.RouteSpec, defaults: new
                    {
                        controller = "DynamicEndpoint",
                        action = "MethodHandler",
                        endpointSpecification = (UserEndpoint)endpointSpecification
                    });
                }

                ConventionalRoutingSwaggerGen.UseRoutes(endpoints);
            });
        }
    }
}