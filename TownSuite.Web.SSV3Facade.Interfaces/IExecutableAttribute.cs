using System;
using System.Reflection;

namespace TownSuite.Web.SSV3Facade.Interfaces
{
    public interface IExecutableAttribute
    {
        Task ExecuteAsync((Type Service, MethodInfo Method,
            Type DtoType)? serviceInfo, object? request);
        int StatusCode { get; }
    }
}

