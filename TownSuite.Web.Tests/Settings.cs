using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TownSuite.Web.Example.ServiceStackExample;
using TownSuite.Web.SSV3Adapter;

namespace TownSuite.Web.Tests;

internal static class Settings
{
    public static ServiceStackV3AdapterOptions GetSettings()
    {
        return new ServiceStackV3AdapterOptions(
            new[]
            {
                typeof(BaseServiceExample)
            })
        {
            RoutePath = "/service/json/syncreply/{name}",
            SerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ContractResolver = new AllPropertiesResolver(),
                Converters =
                {
                    new BackwardsCompatStringConverter()
                }
            },
            SearchAssemblies = new[]
            {
                Assembly.Load("TownSuite.Web.Example")
            }
        };
    }

    public static IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddTransient<IHttpContextAccessor, StubHttpContextAccessor>();


        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}

internal class StubHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext
    {
        get => null;
        set => throw new NotImplementedException();
    }
}