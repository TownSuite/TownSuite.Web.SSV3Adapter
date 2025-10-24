using System.Reflection;
using TownSuite.Web.SSV3Adapter.Interfaces;

namespace TownSuite.Web.Example.ServiceStackExample;

[AttributeUsage(AttributeTargets.Class)]
public class ExamplePropertySetAttribute : Attribute, IExecutableAttribute
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    public string[]? ExampleProperty { get; set; }
    public int StatusCode { get; private set; }

    [ExampleIgnoreConstructor]
    public ExamplePropertySetAttribute(params string[] exampleProperty)
    {
        ExampleProperty = exampleProperty;
    }

    public ExamplePropertySetAttribute(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task ExecuteAsync((Type Service, MethodInfo Method,
        Type DtoType)? serviceInfo, object? request)
    {
        if (request is ExampleAttributeProperty propertyRequest)
        {
            propertyRequest.AttributeValue = ExampleProperty?.Aggregate((acc, val) => $"{acc} {val}");
            StatusCode = StatusCodes.Status200OK;
        }
        else
        {
            StatusCode = StatusCodes.Status501NotImplemented;
        }
        return Task.CompletedTask;
    }
}