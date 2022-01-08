using System;
using Microsoft.AspNetCore.Http;

namespace TownSuite.Web.SSV3Facade
{
    public interface IExecutableAttribute
    {
        Task ExecuteAsync();
        int StatusCode { get; }
    }
}

