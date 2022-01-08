
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;


[assembly: InternalsVisibleTo("TownSuite.Web.Tests")]
namespace TownSuite.Web.SSV3Facade
{

    internal class ServiceStackFacade
    {
        private readonly ServiceStackV3FacadeOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly SsHelper _ssHelper;

        public ServiceStackFacade(ServiceStackV3FacadeOptions options,
            IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _ssHelper = new SsHelper(options, serviceProvider);
        }

        public async Task<(int statusCode, string? json)> Post(string path, string value)
        {

            // TODO: make sure when calling have the front end code include the full namespace of the dto
            // For example call https://localhost/ss/index/Some.Namespace.And.Type

            string name = path?.Split('/')?.LastOrDefault() ?? "";

            if (string.Equals(name, ""))
            {
                return (400, "Service not specified");
            }
            // magic routing based on the name starts here



            var serviceInfo = _ssHelper.GetService(name);

            if (!serviceInfo.HasValue)
            {
                return (404, "Service Not found");
            }

            if (serviceInfo.Value.Method == null || serviceInfo.HasValue == false)
            {
                return (404, "Method not found");
            }

            object? request = JsonConvert.DeserializeObject(value ?? "", serviceInfo.Value.DtoType,
                _options.SerializerSettings);

            return await CreateAndInvokeService(serviceInfo, request);

        }

        private async Task<(int statusCode, string? json)> CreateAndInvokeService((Type Service, MethodInfo Method,
            Type DtoType)? serviceInfo, object? request)
        {
            var secureAttribute = await _ssHelper.GetAttributeAsync<IExecutableAttribute>(serviceInfo.Value.Service);
            if (secureAttribute != null)
            {
                await secureAttribute.ExecuteAsync();
                if (secureAttribute.StatusCode < 200 && secureAttribute.StatusCode >= 300)
                {
                    return (secureAttribute.StatusCode, null);
                }
            }
            var authorizationAttribute = await _ssHelper.GetAttributeAsync<IAuthorizationFilter>(serviceInfo.Value.Service);
            if (authorizationAttribute != null)
            {
                var context = _serviceProvider.GetService<AuthorizationFilterContext>();

                // TODO: what to do here?
                authorizationAttribute.OnAuthorization(context);

            }

            object instance = await _ssHelper.ConstructServiceObjectAsync(serviceInfo.Value.Service);
            if (!_ssHelper.IsServiceType(instance.GetType()))
            {
                throw new NotImplementedException("Only service type objects supported.");
            }

            if (_options.CustomCallBack != null)
            {
                await _options.CustomCallBack((CustomCall.ServiceInstantiated, instance, request));
            }


            object[] arguments = new object[] { request };

            string output;
            Task t = null;
            try
            {
                if (SsHelper.IsAsyncMethod(serviceInfo.Value.Method))
                {
                    t = serviceInfo.Value.Method.Invoke(instance, arguments) as Task;
                    await t.ConfigureAwait(false);
                    var response = t.GetType().GetProperty("Result").GetValue(t);
                    output = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                    return (200, output);
                }
                else
                {
                    var val = await Task.FromResult(serviceInfo.Value.Method.Invoke(instance, arguments));
                    t = Task.FromResult(val);
                    output = Newtonsoft.Json.JsonConvert.SerializeObject(val);
                    return (200, output);
                }
            }
            catch (Exception ex)
            {
                if (_options.CustomErrorHandler != null)
                {
                    var results = await _options.CustomErrorHandler(serviceInfo, instance, ex);
                    if (results.ReThrow)
                    {
                        throw;
                    }
                    output = results.Output;
                }
                else if (t != null)
                {

                    await t.ConfigureAwait(false);
                    output = Newtonsoft.Json.JsonConvert.SerializeObject(t.GetType().GetProperty("Result").GetValue(t)
                        );
                }
                else
                {
                    output = "";
                }

                return (200, output);
            }

        }

    }


}