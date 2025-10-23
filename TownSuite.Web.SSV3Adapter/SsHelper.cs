using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using TownSuite.Web.SSV3Adapter.Interfaces;

namespace TownSuite.Web.SSV3Adapter;

internal class SsHelper
{
    private static readonly ConcurrentDictionary<string, (Type Service, MethodInfo Method, Type DtoType)> ServiceMap =
        new();


    private static readonly ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)>
        SwaggerServiceMap
            = new();

    private readonly ServiceStackV3AdapterOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public SsHelper(ServiceStackV3AdapterOptions options,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public async Task<object> ConstructServiceObjectAsync(Type theService)
    {
        object instance;

        // use constructor with most parameters
        var ctors = theService.GetConstructors();
        // assuming class A has only one constructor
        var ctor = ctors
            .Where(ConstructorAvailable)
            .OrderByDescending(p => p.GetParameters().Count())
            .FirstOrDefault();

        if (ctor.GetParameters().Count() == 0)
        {
            // default contructor
            instance = Activator.CreateInstance(theService);
            return instance;
        }

        var ctorParameters = await InitalizeParameters(ctor);
        instance = ctor.Invoke(ctorParameters.ToArray());
        return instance;
    }

    private async Task<List<object>> InitalizeParameters(ConstructorInfo? ctor)
    {
        var ctorParameters = new List<object>();
        foreach (var param in ctor.GetParameters())
        {
            var parameter = _serviceProvider.GetService(param.ParameterType) 
                ?? throw new NotImplementedException($"{param.ParameterType} not found");

            if (_options.CustomCallBack != null) await _options.CustomCallBack((CustomCall.Parameter, parameter, null));

            ctorParameters.Add(parameter);
        }
        return ctorParameters;
    }

    public (Type Service, MethodInfo Method, Type DtoType)?
        GetService(string requestName)
    {
        // TODO: ADD CACHE
        if (ServiceMap.ContainsKey(requestName)) return ServiceMap[requestName];

        foreach (var asm in _options.SearchAssemblies)
        {
            var typeInfo = asm.GetTypes().Where(p => IsServiceType(p)).OrderBy(p => p.Name);
            foreach (var service in typeInfo)
            {
                var methodInfo = GetMethod(requestName, service);
                if (methodInfo.method != null)
                {
                    // key, value, func<TKey, TValue, TValue>
                    ServiceMap.AddOrUpdate(requestName,
                        (service, methodInfo.method, methodInfo.dtoType),
                        (s, m) => { return (service, methodInfo.method, methodInfo.dtoType); });

                    return (service, methodInfo.method, methodInfo.dtoType);
                }
            }

            // continue on and try the next dll
        }

        return null;
    }

    public ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)>
        GetAllServices()
    {
        if (SwaggerServiceMap.Any()) return SwaggerServiceMap;

        var types = PermissiveLoadAssemblies();

        var typeInfo = types.Where(p => IsServiceType(p)).OrderBy(p => p.Name);
        foreach (var service in typeInfo)
        {
            var methodInfo = GetMethod("", service);
            if (methodInfo.method != null)
                // key, value, func<TKey, TValue, TValue>
                SwaggerServiceMap.AddOrUpdate(methodInfo.method.DeclaringType,
                    (service, methodInfo.method, methodInfo.dtoType),
                    (s, m) => { return (service, methodInfo.method, methodInfo.dtoType); });
        }

        // continue on and try the next dll
        return SwaggerServiceMap;
    }

    private List<Type> PermissiveLoadAssemblies()
    {
        List<Type> types = new List<Type>();

        int assemblyCount = _options.SearchAssemblies.Length;

        for (int i = 0; i < assemblyCount; i++)
        {
            try
            {
                types.AddRange(_options.SearchAssemblies[i].GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle assemblies that cannot load all types
                foreach (Type theType in ex.Types)
                {
                    try
                    {
                        types.Add(theType);
                    }
                    catch (Exception)
                    {
                        // Type not in this assembly - reference to elsewhere ignored
                    }
                }
            }
        }

        return types;
    }


    public static bool IsAsyncMethod(MethodInfo method)
    {
        // see https://stackoverflow.com/questions/20350397/how-can-i-tell-if-a-c-sharp-method-is-async-await-via-reflection


        var attType = typeof(AsyncStateMachineAttribute);

        // Obtain the custom attribute for the method. 
        // The value returned contains the StateMachineType property. 
        // Null is returned if the attribute isn't present for the method. 
        var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

        return attrib != null;
    }


    public bool IsServiceType(Type instance)
    {
        foreach (var item in _options.ServiceTypes)
        {
            if (instance == null)
                continue;
            if (instance.BaseType == item)
                return true;
            else if (instance.IsSubclassOf(item))
                return true;
            else if (instance.IsAssignableTo(item) && instance != item)
                return true;
        }

        return false;
    }


    public static (MethodInfo method, Type dtoType) GetMethod(string requestName, Type theService)
    {
        var methods = GetActions(requestName, theService);
        MethodInfo method = null;
        Type paramType = null;
        foreach (var mi in methods)
        {
            var parameters = mi.GetParameters();
            if (parameters.Length != 1) continue;

            var parameterString = parameters.FirstOrDefault().ParameterType.ToString();
            var individualParameter = parameterString?.Split('.').LastOrDefault();
            if (individualParameter == requestName ||
                string.IsNullOrWhiteSpace(requestName))
            {
                method = mi;
                paramType = parameters.FirstOrDefault().ParameterType;
            }
        }

        return (method, paramType);
    }

    public async Task<T?> GetAttributeAsync<T>(Type service)
    {
        var attribute = Attribute.GetCustomAttributes(
            service)?.Where(p => p.GetType().GetInterfaces().Contains(typeof(T)))?.FirstOrDefault();
        var secureAttType = attribute?.GetType();
        if (secureAttType == null) return default;

        var ctors = secureAttType.GetConstructors();
        // assuming class A has only one constructor
        var ctor = ctors
            .Where(ConstructorAvailable)
            .OrderByDescending(p => p.GetParameters().Count())
            .FirstOrDefault();

        var ctorParameters = await InitalizeParameters(ctor);

        var instance = ctor.Invoke(ctorParameters.ToArray());
        SetNewObjectsProperties(attribute!, instance, secureAttType);
        return (T)instance;
    }

    private static bool ConstructorAvailable(ConstructorInfo constructor)
    {
        var hasIgnoreAttribute = constructor?.GetCustomAttributes()
            ?.Any(p => p.GetType().GetInterfaces().Contains(typeof(IIgnoreConstructorAttribute)));
        return !hasIgnoreAttribute.HasValue || !hasIgnoreAttribute.Value;
    }

    private static void SetNewObjectsProperties(object existingObject, object newObject, Type type)
    {
        var properties = type.GetProperties()
                    .Where(props => props.CanRead && props.CanWrite);
        foreach (var prop in properties)
        {
            prop.SetValue(newObject, prop.GetValue(existingObject));
        }
    }

    public async Task<IExecutableAttribute> GetDescriptionAttributeAsync(Type service, object dto)
    {
        var secureAttType = Attribute.GetCustomAttributes(
                service)?.Where(p => p.GetType().GetInterfaces().Contains(typeof(DescriptionAttribute)))
            ?.FirstOrDefault()
            ?.GetType();
        if (secureAttType == null) return null;

        var ctors = secureAttType.GetConstructors();
        // assuming class A has only one constructor
        var ctor = ctors
            .Where(ConstructorAvailable)
            .OrderByDescending(p => p.GetParameters().Count())
            .FirstOrDefault();

        var ctorParameters = await InitalizeParameters(ctor);

        var instance = ctor.Invoke(ctorParameters.ToArray());
        return (IExecutableAttribute)instance;
    }

    public static IEnumerable<MethodInfo> GetActions(string requestName, Type serviceType)
    {
        foreach (var mi in serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name))
        {
            if (mi.IsGenericMethod || mi.GetParameters().Length != 1)
                continue;

            var paramType = mi.GetParameters()[0].ParameterType;
            if (paramType.IsValueType || paramType == typeof(string))
                continue;

            var actionName = mi.Name.ToUpper();
            if (actionName != "ANY" && actionName != "ANYASYNC" &&
                actionName != "POST" && actionName != "GET")
            {
                if (string.Equals(paramType.Name, requestName, StringComparison.InvariantCultureIgnoreCase))
                    yield return mi;

                continue;
            }

            yield return mi;
        }
    }
}