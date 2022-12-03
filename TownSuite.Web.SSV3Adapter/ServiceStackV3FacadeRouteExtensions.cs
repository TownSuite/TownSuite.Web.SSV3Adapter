using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TownSuite.Web.SSV3Adapter;

public static class ServiceStackV3AdapterRouteExtensions
{
    public static void UseServiceStackV3Adapter(
        this IApplicationBuilder applicationBuilder,
        ServiceStackV3AdapterOptions options,
        IServiceProvider serviceProvider)
    {
        var builder = new RouteBuilder(applicationBuilder);

        // use middlewares to configure a route
        builder.MapMiddlewarePost(options.RoutePath, appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                using var prom = options?.Prometheus?.Invoke();

                string path = context.Request.Path;

                string value;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    value = await reader.ReadToEndAsync();
                }

                var method = context.Request.Method;

                var Adapter = new ServiceStackAdapter(options,
                    serviceProvider,
                    prom);
                var results = await Adapter.Post(path, value, method);

                context.Response.StatusCode = results.statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(results.json ?? "");
            });
        });

        applicationBuilder.UseRouter(builder.Build());
    }


    public static void UseServiceStackV3AdapterSwagger(
        this IApplicationBuilder applicationBuilder,
        ServiceStackV3AdapterOptions options, string description = "Description",
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
                var host = $"{context.Request.Host}{context.Request.PathBase}";
                var results = await swag.Generate(host);

                context.Response.StatusCode = results.statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(results.json ?? "");
            });
        });

        applicationBuilder.UseRouter(builder.Build());
    }
}