﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting;

namespace AnyRest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddXmlSerializerFormatters();

            services.AddLogging(c => c.ClearProviders());

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

            var userEndpoints = (EndpointList)app.ApplicationServices.GetService(typeof(EndpointList));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                var logger = new ConsoleLogger();
                foreach (Endpoint userEndpoint in userEndpoints)
                {
                    endpoints.Map(userEndpoint.FullRoute, (HttpContext context) => { return userEndpoint.HandleRequest(context, logger); });
                }
                endpoints.MapFallback((HttpContext context) => { return Endpoint.HandleFallBackRequest(context, logger); });

                ConventionalRoutingSwaggerGen.UseRoutes(endpoints);
            });
        }
    }
}