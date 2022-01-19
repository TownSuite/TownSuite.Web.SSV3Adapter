using System;

namespace TownSuite.Web.SSV3Facade.Interfaces
{
    public interface IExecutableAttribute
    {
        Task ExecuteAsync();
        int StatusCode { get; }
    }
}

