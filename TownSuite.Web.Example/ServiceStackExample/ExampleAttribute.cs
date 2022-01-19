using System;
using TownSuite.Web.SSV3Facade;
using TownSuite.Web.SSV3Facade.Interfaces;

namespace TownSuite.Web.Example.ServiceStackExample
{
	public class ExampleAttribute : Attribute,  IExecutableAttribute
	{
        readonly IHttpContextAccessor _httpContextAccessor;

        public ExampleAttribute()
        {
           
        }

        public ExampleAttribute(IHttpContextAccessor httpContextAccessor)
		{
            _httpContextAccessor = httpContextAccessor;
		}

        public int StatusCode { get; private set; } = 0;

        public Task ExecuteAsync()
        {
            StatusCode = 200;
            return Task.CompletedTask;
        }
    }
}

