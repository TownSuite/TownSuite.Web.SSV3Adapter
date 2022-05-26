using System;
using System.Reflection;
using TownSuite.Web.SSV3Facade;
using TownSuite.Web.SSV3Facade.Interfaces;

namespace TownSuite.Web.Example.ServiceStackExample
{
    public interface NestedInterface : IExecutableAttribute
    {
        public string StatusMessage { get; set; }
    }

    public class NestedExampleAttribute : Attribute, NestedInterface
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        const string TestingValue = "was this method called";

        public NestedExampleAttribute()
        {
        }

        public NestedExampleAttribute(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public int StatusCode { get; private set; } = 0;

        public Task ExecuteAsync((Type Service, MethodInfo Method,
            Type DtoType)? serviceInfo, object? request)
        {
            if (request.ToString().Contains(TestingValue))
            {
                StatusMessage = "Nested executor attribute was called";
                StatusCode = 200;
            }
            else
            {
                StatusMessage = "Try again";
                StatusCode = 400;
            }


            return Task.CompletedTask;
        }

        public string StatusMessage { get; set; }
        
    }
}