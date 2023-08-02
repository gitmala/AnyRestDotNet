﻿using Microsoft.AspNetCore.Builder;
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

            var userEndpoints = (Endpoints)app.ApplicationServices.GetService(typeof(Endpoints));

            app.UseEndpoints(endpoints =>
            {
                foreach (Endpoint endpointSpecification in userEndpoints)
                {
                    endpoints.MapControllerRoute(endpointSpecification.Id, endpointSpecification.RouteSpec, defaults: new
                    {
                        controller = "DynamicEndpoint",
                        action = "MethodHandler",
                        endpointSpecification = (Endpoint)endpointSpecification
                    });
                }

                ConventionalRoutingSwaggerGen.UseRoutes(endpoints);
            });
        }
    }
}