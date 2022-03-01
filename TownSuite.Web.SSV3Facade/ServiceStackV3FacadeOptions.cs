using System.Reflection;

namespace TownSuite.Web.SSV3Facade
{
    public class ServiceStackV3FacadeOptions
    {
        public ServiceStackV3FacadeOptions(Type[] serviceTypes)
        {
            ServiceTypes = serviceTypes;
            SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
        }

        public string RoutePath { get; set; } = "/service/json/syncreply/{name}";

        /// <summary>
        /// Base class services inherit.  
        /// </summary>
        public Type[] ServiceTypes { get; set; }

        /// <summary>
        /// Optional callback method in the case that custom error processing is required.
        /// </summary>
        public Func<(Type Service, MethodInfo Method, Type DtoType)?, object, Exception,
            Task<(string Output, bool ReThrow)>>? CustomErrorHandler
        { get; set; }

        public Newtonsoft.Json.JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Array of assemblies that will be searched to locate request, response, and service classes.
        /// </summary>
        public Assembly[] SearchAssemblies { get; set; }

        /// <summary>
        /// Inject custom code at various locations.
        ///
        /// Input 
        /// </summary>
        public Func<(CustomCall callbackType, object serviceInstance,
            object? requestDto), Task> CustomCallBack { get; set; }

        public Func<Interfaces.ISSV3Promethues>? Promethues { get; set; } = null;

    }
}

