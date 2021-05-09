using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lazarus.Common.Infrastructure
{

    public static class Swagger
    {
        public static void Gen(IConfiguration configuration, SwaggerGenOptions c, string moduleName, string version = "v1")
        {

            c.IgnoreObsoleteProperties();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            env = string.IsNullOrEmpty(env) ? "" : "ENVIRONMENT:" + env + ", ";
            c.SwaggerDoc(version, new OpenApiInfo
            {
                Title = "OMS Legacy " + moduleName+ " API",
                Version = version,
                Description = env + "BUILD_VERSION:" + Environment.GetEnvironmentVariable("BUILD_VERSION")
            });

            var securityOauth2 = new OpenApiSecurityScheme
            {
                Description = "Username password",
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Scheme = "bearer",
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri(configuration["AuthenticationUri"] + "token"),
                        AuthorizationUrl = new Uri(configuration["AuthenticationUri"]),
                        RefreshUrl = new Uri(configuration["AuthenticationUri"] + "refreshtoken"),
                    }
                },
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Oauth2",
                }
            };
            c.AddSecurityDefinition("Oauth2", securityOauth2);

            var securitySchemaBasic = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                In = ParameterLocation.Header,
                Description = "Basic Authorization header using the Bearer scheme.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            };
            c.AddSecurityDefinition("basic", securitySchemaBasic);

            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header \"Authorization: Bearer {token}\"",
                Name = "authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri(configuration["AuthenticationUri"] + "token"),
                        AuthorizationUrl = new Uri(configuration["AuthenticationUri"]),
                        RefreshUrl = new Uri(configuration["AuthenticationUri"] + "refreshtoken"),
                    },
                },
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            };
            c.AddSecurityDefinition("Bearer", securitySchema);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityOauth2, new[] { "Oauth2" } },
                { securitySchemaBasic, new[]  {"basic"}},
                { securitySchema, new[] { "Bearer" } },
            });
        }

        public static void Use(IConfiguration configuration, SwaggerOptions c)
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                if (Environment.MachineName.StartsWith("CPX-"))
                {
                    swaggerDoc.Servers = new List<OpenApiServer> { 
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{httpReq.PathBase}" }
                    };
                }
                else
                {
                    swaggerDoc.Servers = new List<OpenApiServer> { 
                        new OpenApiServer { Url = $"{configuration["SwaggerBaseUrl"]}" },
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Host}{configuration["VirtualDirectory"]}" } 
                    };
                }
            });
        }

        public static void UseSwaggerUI(SwaggerUIOptions c, IWebHostEnvironment env, string moduleName = "", string version = "v1")
        {
            c.SwaggerEndpoint(version + "/swagger.json", "OMSLegacy: " + moduleName + ", Env:" + env.EnvironmentName);
            c.DocExpansion(DocExpansion.None);
            c.OAuthClientId("");
            c.OAuthClientSecret("");
        }
    }
}
