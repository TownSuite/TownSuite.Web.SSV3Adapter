using System.Reflection;

namespace TownSuite.Web.SSV3Adapter.Interfaces;

public interface IExecutableAttribute
{
    int StatusCode { get; }

    Task ExecuteAsync((Type Service, MethodInfo Method,
        Type DtoType)? serviceInfo, object? request);
}