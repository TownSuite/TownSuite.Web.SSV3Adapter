using System.Reflection;
using TownSuite.Web.SSV3Adapter.Interfaces;

namespace TownSuite.Web.Example.ServiceStackExample;

public class ExampleAttribute : Attribute, IExecutableAttribute
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExampleAttribute()
    {
    }

    public ExampleAttribute(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int StatusCode { get; private set; }

    public Task ExecuteAsync((Type Service, MethodInfo Method,
        Type DtoType)? serviceInfo, object? request)
    {
        StatusCode = 200;
        return Task.CompletedTask;
    }
}