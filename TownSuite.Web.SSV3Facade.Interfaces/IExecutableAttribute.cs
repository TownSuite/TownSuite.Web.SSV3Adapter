using System.Reflection;

namespace TownSuite.Web.SSV3Facade.Interfaces;

public interface IExecutableAttribute
{
    int StatusCode { get; }

    Task ExecuteAsync((Type Service, MethodInfo Method,
        Type DtoType)? serviceInfo, object? request);
}