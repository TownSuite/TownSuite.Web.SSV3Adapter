using System.Reflection;
using TownSuite.Web.SSV3Facade.Interfaces;

namespace TownSuite.Web.Example.ServiceStackExample;

public interface NestedInterface : IExecutableAttribute
{
    public string StatusMessage { get; set; }
}

public class NestedExampleAttribute : Attribute, NestedInterface
{
    private const string TestingValue = "was this method called";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NestedExampleAttribute()
    {
    }

    public NestedExampleAttribute(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public int StatusCode { get; private set; }

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