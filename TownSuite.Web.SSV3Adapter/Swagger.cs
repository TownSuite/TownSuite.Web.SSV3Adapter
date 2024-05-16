using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace TownSuite.Web.SSV3Adapter;

internal class Swagger
{
    private static string? jsonCached;
    private readonly string _description;
    private readonly ServiceStackV3AdapterOptions _options;
    private readonly SsHelper _ssHelper;
    private readonly string _title;
    private readonly string _version;
    private AssemblyBuilder ab;
    private AssemblyName aName;
    private ModuleBuilder mb;
    private TypeBuilder tb;


    private Type type;

    public Swagger(ServiceStackV3AdapterOptions options,
        IServiceProvider serviceProvider, string description,
        string title, string version)
    {
        _options = options;
        _ssHelper = new SsHelper(options, serviceProvider);
        _description = description;
        _title = title;
        _version = version;
    }

    public async Task<(int statusCode, string json)> Generate(string host)
    {
        if (!string.IsNullOrWhiteSpace(jsonCached)) return (200, jsonCached ?? "");

        await PreGenerateJson(host);

        return (200, jsonCached ?? "");
    }

    /// <summary>
    ///     Used to pre
    /// </summary>
    private async Task PreGenerateJson(string host)
    {
        var serviceInfo = _ssHelper.GetAllServices();

        var swaggerDoc = new OpenApiDocument();
        swaggerDoc.Info = new OpenApiInfo()
        {
            Title = "TownSuite.SSV3Adapter",
            Version = "v1"
        };
        swaggerDoc.Components = new OpenApiComponents();
        swaggerDoc.Components.Schemas = new Dictionary<string, OpenApiSchema>();
        swaggerDoc.Paths = new OpenApiPaths();

        HashSet<Type> processedTypes = new HashSet<Type>();
        foreach (var service in serviceInfo)
        {
            var descriptionAttribute = await _ssHelper.GetAttributeAsync<DescriptionAttribute>(
                service.Value.Service
            );

            var requestProp = service.Value.DtoType;
            Type responseProp;
            var requestProperties = service.Value.DtoType.GetProperties();
            PropertyInfo[] responseProperties;
            if (SsHelper.IsAsyncMethod(service.Value.Method))
            {
                responseProp = service.Value.Method.ReturnType
                    .GetProperties().FirstOrDefault().PropertyType;
                responseProperties = responseProp.GetProperties();
            }
            else
            {
                responseProp = service.Value.Method.ReturnType;
                responseProperties = responseProp.GetProperties();
            }
            
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            var requestSchema = new OpenApiSchema
            {
                Type = "object",
                Description = descriptionAttribute?.Description ?? "",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var prop in requestProperties)
            {
                var s = GetOpenApiSchema(prop.PropertyType, 0, processedTypes);
                if (s != null && !requestSchema.Properties.ContainsKey(prop.Name))
                {
                    requestSchema.Properties.Add(prop.Name, s);
                }
            }

            foreach (var prop in responseProperties)
            {
                if (!schema.Properties.ContainsKey(prop.Name))
                {
                    schema.Properties.Add(prop.Name, GetOpenApiSchema(prop.PropertyType, 0, processedTypes));
                }
            }

            // Add the schema to the Swagger document
            string endpointName = requestProp.Name;
            string theNamespace = requestProp.Namespace;
            string responseName = responseProp.Name;
            string responseNamespace = responseProp.Namespace;

            if (!swaggerDoc.Paths.ContainsKey($"{_options.RoutePath}/{endpointName}"))
            {
                swaggerDoc.Paths.Add($"{_options.RoutePath}/{endpointName}", new OpenApiPathItem()
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>()
                    {
                        [OperationType.Post] = new OpenApiOperation()
                        {
                            RequestBody = new OpenApiRequestBody()
                            {
                                Content = new Dictionary<string, OpenApiMediaType>()
                                {
                                    ["application/json"] = new OpenApiMediaType()
                                    {
                                        Schema = new OpenApiSchema()
                                        {
                                            Reference = new OpenApiReference()
                                            {
                                                Id = $"{theNamespace}.{endpointName}",
                                                Type = ReferenceType.Schema
                                            }
                                        }
                                    }
                                }
                            },
                            Responses = new OpenApiResponses()
                            {
                                ["200"] = new OpenApiResponse()
                                {
                                    Description = "Success",
                                    Content = new Dictionary<string, OpenApiMediaType>()
                                    {
                                        ["application/json"] = new OpenApiMediaType()
                                        {
                                            Schema = new OpenApiSchema()
                                            {
                                                Reference = new OpenApiReference()
                                                {
                                                    Id = $"{responseNamespace}.{responseName}",
                                                    Type = ReferenceType.Schema
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            if (!swaggerDoc.Components.Schemas.ContainsKey($"{theNamespace}.{endpointName}"))
            {
                swaggerDoc.Components.Schemas.Add($"{theNamespace}.{endpointName}",
                    requestSchema);
            }

            if (!swaggerDoc.Components.Schemas.ContainsKey($"{responseNamespace}.{responseName}"))
            {
                swaggerDoc.Components.Schemas.Add($"{responseNamespace}.{responseName}",
                    schema);
            }
        }

        var sb = new StringBuilder();
        swaggerDoc.SerializeAsV3(new OpenApiJsonWriter(new StringWriter(sb)));
        jsonCached = sb.ToString();
    }

    private OpenApiSchema GetOpenApiSchema(Type type, int level, HashSet<Type> processedTypes)
    {
        if (level > 10)
        {
            return new OpenApiSchema { Description = "Recursion limit reached" };
        }

        if (processedTypes.Contains(type))
        {
            return null;
        }

        if (type == typeof(string))
        {
            return new OpenApiSchema { Type = "string" };
        }

        if (type == typeof(int))
        {
            return new OpenApiSchema { Type = "integer" };
        }

        if (type == typeof(bool))
        {
            return new OpenApiSchema { Type = "boolean" };
        }

        if (type == typeof(DateTime))
        {
            return new OpenApiSchema { Type = "string" };
        }

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return new OpenApiSchema { Type = "number" };
        }

        // If the type is a custom object
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };

        processedTypes.Add(type);
        foreach (var prop in type.GetProperties())
        {
            if (!schema.Properties.ContainsKey(prop.Name))
            {
                var s = GetOpenApiSchema(prop.PropertyType, level + 1, processedTypes);
                if (s != null)
                {
                    schema.Properties.Add(prop.Name, s);
                }
            }
        }

        return schema;
    }
}