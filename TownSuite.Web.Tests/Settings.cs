using System;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TownSuite.Web.SSV3Facade;

namespace TownSuite.Web.Tests
{
    internal static class Settings
    {

        public static ServiceStackV3FacadeOptions GetSettings()
        {

            return new ServiceStackV3FacadeOptions(
                serviceTypes: new Type[] {
                    typeof(TownSuite.Web.Example.ServiceStackExample.BaseServiceExample)
                })
            {
                RoutePath = "/service/json/syncreply/{name}",
                SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
                    ContractResolver = new AllPropertiesResolver(),
                    Converters = {
                        new BackwardsCompatStringConverter()
                    }
                },
                SearchAssemblies = new Assembly[]
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
        public HttpContext? HttpContext { get => null; set => throw new NotImplementedException(); }
    }
}

