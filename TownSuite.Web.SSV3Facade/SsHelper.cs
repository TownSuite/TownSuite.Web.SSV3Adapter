using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using TownSuite.Web.SSV3Facade.Interfaces;

namespace TownSuite.Web.SSV3Facade
{

    internal class SsHelper
    {

        private readonly ServiceStackV3FacadeOptions _options;
        private readonly IServiceProvider _serviceProvider;
        public SsHelper(ServiceStackV3FacadeOptions options,
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
            var ctor = ctors.OrderByDescending(p => p.GetParameters().Count()).FirstOrDefault();

            if (ctor.GetParameters().Count() == 0)
            {
                // default contructor
                instance = Activator.CreateInstance(theService);
                return instance;
            }

            var ctorParameters = new List<object>();
            await InitalizeParameters(ctor, ctorParameters);
            instance = ctor.Invoke(ctorParameters.ToArray());
            return instance;
        }

        private async Task InitalizeParameters(ConstructorInfo? ctor, List<object> ctorParameters)
        {
            foreach (var param in ctor.GetParameters())
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var parameter = scope.ServiceProvider.GetService(param.ParameterType);
                    if (parameter == null)
                    {
                        throw new NotImplementedException($"{param.ParameterType.ToString()} not found");
                    }

                    if (_options.CustomCallBack != null)
                    {
                        await _options.CustomCallBack((CustomCall.Parameter, parameter, null));
                    }

                    ctorParameters.Add(parameter);
                }
            }
        }

        private static ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)> ServiceMap
            = new ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)>();

        public (Type Service, MethodInfo Method, Type DtoType)?
            GetService(string requestName)
        {

            foreach (Assembly asm in _options.SearchAssemblies)
            {

                var typeInfo = asm.GetTypes().Where(p => IsServiceType(p)).OrderBy(p => p.Name);
                foreach (var service in typeInfo)
                {
                    var methodInfo = GetMethod(requestName, service);
                    if (methodInfo.method != null)
                    {
                        // key, value, func<TKey, TValue, TValue>
                        ServiceMap.AddOrUpdate(methodInfo.method.DeclaringType,
                            (service, methodInfo.method, methodInfo.dtoType), (s, m) =>
                            {
                                return (service, methodInfo.method, methodInfo.dtoType);
                            });

                        return (service, methodInfo.method, methodInfo.dtoType);
                    }
                }

                // continue on and try the next dll
            }

            return null;
        }


        private static ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)> SwaggerServiceMap
            = new ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)>();

        public ConcurrentDictionary<Type, (Type Service, MethodInfo Method, Type DtoType)>
           GetAllServices()
        {
            if (SwaggerServiceMap.Any())
            {
                return SwaggerServiceMap;
            }

            foreach (Assembly asm in _options.SearchAssemblies)
            {
                var typeInfo = asm.GetTypes().Where(p => IsServiceType(p)).OrderBy(p => p.Name);
                foreach (var service in typeInfo)
                {
                    var methodInfo = GetMethod("", service);
                    if (methodInfo.method != null)
                    {
                        // key, value, func<TKey, TValue, TValue>
                        SwaggerServiceMap.AddOrUpdate(methodInfo.method.DeclaringType,
                            (service, methodInfo.method, methodInfo.dtoType), (s, m) =>
                            {
                                return (service, methodInfo.method, methodInfo.dtoType);
                            });
                    }
                }

                // continue on and try the next dll
            }

            return SwaggerServiceMap;
        }


        public static bool IsAsyncMethod(MethodInfo method)
        {
            // see https://stackoverflow.com/questions/20350397/how-can-i-tell-if-a-c-sharp-method-is-async-await-via-reflection


            Type attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }


        public bool IsServiceType(Type instance)
        {

            foreach (var item in _options.ServiceTypes)
            {
                if (instance.BaseType == item)
                {
                    return true;
                }
                else if (instance.IsSubclassOf(item))
                {
                    return true;
                }
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
                if (parameters.Length != 1)
                {
                    continue;
                }

                if (parameters.FirstOrDefault().ParameterType.ToString()?.Split('.').LastOrDefault() == requestName ||
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

            var secureAttType = Attribute.GetCustomAttributes(
            service)?.Where(p => p.GetType().GetInterfaces().Contains(typeof(T)))?.FirstOrDefault()?.GetType();
            if (secureAttType == null)
            {
                return default(T);
            }

            var ctors = secureAttType.GetConstructors();
            // assuming class A has only one constructor
            var ctor = ctors.OrderByDescending(p => p.GetParameters().Count()).FirstOrDefault();


            var ctorParameters = new List<object>();
            await InitalizeParameters(ctor, ctorParameters);

            var instance = ctor.Invoke(ctorParameters.ToArray());
            return (T)instance;
        }


        public async Task<IExecutableAttribute> GetDescriptionAttributeAsync(Type service, object dto)
        {

            var secureAttType = Attribute.GetCustomAttributes(
            service)?.Where(p => p.GetType().GetInterfaces().Contains(typeof(DescriptionAttribute)))?.FirstOrDefault()?.GetType();
            if (secureAttType == null)
            {
                return null;
            }

            var ctors = secureAttType.GetConstructors();
            // assuming class A has only one constructor
            var ctor = ctors.OrderByDescending(p => p.GetParameters().Count()).FirstOrDefault();


            var ctorParameters = new List<object>();
            await InitalizeParameters(ctor, ctorParameters);

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
                    {
                        yield return mi;
                    }

                    continue;
                }

                yield return mi;
            }
        }

    }
}