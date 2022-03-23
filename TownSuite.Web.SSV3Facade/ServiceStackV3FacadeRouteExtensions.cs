using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using System.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace TownSuite.Web.SSV3Facade
{
    public static class ServiceStackV3FacadeRouteExtensions
    {

        public static void UseServiceStackV3Facade(
           this IApplicationBuilder applicationBuilder,
           ServiceStackV3FacadeOptions options,
           IServiceProvider serviceProvider = null)
        {
            var builder = new RouteBuilder(applicationBuilder);

            // use middlewares to configure a route
            builder.MapMiddlewarePost(options.RoutePath, appBuilder =>
            {

                appBuilder.Run(async context =>
                {
                    using var prom = options?.Promethues();

                    string path = context.Request.Path;

                    string value;
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        value = await reader.ReadToEndAsync();
                    }

                    string method = context.Request.Method;

                    var facade = new ServiceStackFacade(options,
                             serviceProvider == null ? builder.ServiceProvider : serviceProvider,
                            prom);
                    var results = await facade.Post(path, value, method);

                    context.Response.StatusCode = results.statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(results.json ?? "");

                });

            });

            applicationBuilder.UseRouter(builder.Build());
        }


        public static void UseServiceStackV3FacadeSwagger(
          this IApplicationBuilder applicationBuilder,
          ServiceStackV3FacadeOptions options, string description = "Description",
          string title = "Title", string version = "1.0.0.0",
          IServiceProvider serviceProvider = null)
        {
            var builder = new RouteBuilder(applicationBuilder);

            // use middlewares to configure a route
            builder.MapMiddlewareGet(options.SwaggerPath, appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    string path = context.Request.Path;

                    string value;
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        value = await reader.ReadToEndAsync();
                    }

                    var swag = new Swagger(options,
                        serviceProvider == null ? builder.ServiceProvider : serviceProvider,
                        description, title, version);
                    //string host = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
                    string host = $"{context.Request.Host}{context.Request.PathBase}";
                    var results = await swag.Generate(host);

                    context.Response.StatusCode = results.statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(results.json ?? "");

                });
            });

            applicationBuilder.UseRouter(builder.Build());
        }
    }
}

