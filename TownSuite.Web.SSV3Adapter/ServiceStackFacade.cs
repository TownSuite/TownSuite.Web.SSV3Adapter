using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TownSuite.Web.SSV3Adapter.Interfaces;

[assembly: InternalsVisibleTo("TownSuite.Web.Tests")]

namespace TownSuite.Web.SSV3Adapter;

internal class ServiceStackAdapter
{
    private readonly ServiceStackV3AdapterOptions _options;
    private readonly ISSV3Prometheus? _prom;
    private readonly IServiceProvider _serviceProvider;
    private readonly SsHelper _ssHelper;

    public ServiceStackAdapter(ServiceStackV3AdapterOptions options,
        IServiceProvider serviceProvider,
        ISSV3Prometheus? prom = null)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _ssHelper = new SsHelper(options, serviceProvider);
        _prom = prom;
    }

    public async Task<(int statusCode, string? json)> Post(
        string path,
        string value,
        string method)
    {
        try
        {
            // TODO: make sure when calling have the front end code include the full namespace of the dto
            // For example call https://localhost/ss/index/Some.Namespace.And.Type

            var name = path.Split('/').LastOrDefault() ?? "";

            if (string.Equals(name, "")) return (400, "Service not specified");
            // magic routing based on the name starts here


            var serviceInfo = _ssHelper.GetService(name);

            if (!serviceInfo.HasValue)
            {
                _prom?.ExceptionTriggered();
                return (404, "Service Not found");
            }

            if (serviceInfo.Value.Method == null || serviceInfo.HasValue == false)
            {
                _prom?.ExceptionTriggered();
                return (404, "Method not found");
            }

            var request = JsonConvert.DeserializeObject(value ?? "", serviceInfo.Value.DtoType,
                _options.SerializerSettings);

            var results = await CreateAndInvokeService(serviceInfo.Value, request);
            _prom?.EndRequest(results.statusCode.ToString(),
                method,
                name, serviceInfo.Value.Method.Name);
            return results;
        }
        catch (Exception ex)
        {
            if (_options.OtherExceptionCallback != null)
            {
                var returnValues = await _options.OtherExceptionCallback(ex);
                if (returnValues != null)
                    return returnValues.Value;
            }
            throw;
        }
    }

    private async Task<(int statusCode, string? json)> CreateAndInvokeService((Type Service, MethodInfo Method,
        Type DtoType) serviceInfo, object? request)
    {
        var secureAttribute = await _ssHelper.GetAttributeAsync<IExecutableAttribute>(serviceInfo.Service);
        if (secureAttribute != null)
        {
            await secureAttribute.ExecuteAsync(serviceInfo, request);
            if (secureAttribute.StatusCode < 200 || secureAttribute.StatusCode >= 300)
                return (secureAttribute.StatusCode, null);
        }

        var authorizationAttribute = await _ssHelper.GetAttributeAsync<IAuthorizationFilter>(serviceInfo.Service);
        if (authorizationAttribute != null)
        {
            var context = _serviceProvider.GetService<AuthorizationFilterContext>();

            // TODO: what to do here?
            authorizationAttribute.OnAuthorization(context);
        }

        var instance = await _ssHelper.ConstructServiceObjectAsync(serviceInfo.Service);
        if (!_ssHelper.IsServiceType(instance.GetType()))
            throw new NotImplementedException("Only service type objects supported.");

        if (_options.CustomCallBack != null)
            await _options.CustomCallBack((CustomCall.ServiceInstantiated, instance, request));


        object[] arguments = { request };

        string output;
        Task t = null;
        try
        {
            if (SsHelper.IsAsyncMethod(serviceInfo.Method))
            {
                t = serviceInfo.Method.Invoke(instance, arguments) as Task;
                await t.ConfigureAwait(false);
                var response = t.GetType().GetProperty("Result").GetValue(t);
                output = JsonConvert.SerializeObject(response);
                return (200, output);
            }

            var val = await Task.FromResult(serviceInfo.Method.Invoke(instance, arguments));
            t = Task.FromResult(val);
            output = JsonConvert.SerializeObject(val);
            return (200, output);
        }
        catch (Exception ex)
        {
            if (_options.CustomErrorHandler != null)
            {
                var results = await _options.CustomErrorHandler(serviceInfo, instance, ex);
                if (results.ReThrow) throw;
                output = results.Output;
            }
            else if (t != null)
            {
                await t.ConfigureAwait(false);
                output = JsonConvert.SerializeObject(t.GetType().GetProperty("Result").GetValue(t)
                );
            }
            else
            {
                output = "";
            }

            return (200, output);
        }
        finally
        {
            t?.Dispose();
        }
    }
}