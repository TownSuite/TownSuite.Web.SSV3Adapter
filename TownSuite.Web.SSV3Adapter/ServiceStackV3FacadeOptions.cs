using System.Reflection;
using Newtonsoft.Json;
using TownSuite.Web.SSV3Adapter.Interfaces;

namespace TownSuite.Web.SSV3Adapter;

public class ServiceStackV3AdapterOptions
{
    public ServiceStackV3AdapterOptions(Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes;
        SerializerSettings = new JsonSerializerSettings();
    }

    public string SwaggerPath { get; set; } = "/swag/swagger.json";
    public string RoutePath { get; set; } = "/service/json/syncreply";

    /// <summary>
    ///     Base class services inherit.
    /// </summary>
    public Type[] ServiceTypes { get; set; }

    /// <summary>
    ///     Optional callback method in the case that custom error processing is required.
    /// </summary>
    public Func<(Type Service, MethodInfo Method, Type DtoType)?, object, Exception,
        Task<(string Output, bool ReThrow)>>? CustomErrorHandler { get; set; }

    public JsonSerializerSettings SerializerSettings { get; set; }

    /// <summary>
    ///     Array of assemblies that will be searched to locate request, response, and service classes.
    /// </summary>
    public Assembly[] SearchAssemblies { get; set; }

    /// <summary>
    ///     Inject custom code at various locations.
    ///     Input
    /// </summary>
    public Func<(CustomCall callbackType, object serviceInstance,
        object? requestDto), Task> CustomCallBack { get; set; }

    public Func<ISSV3Prometheus>? Prometheus { get; set; } = null;
    public Func<Exception, (int statusCode, string? json)?> OtherExceptionCallback { get; internal set; }
}