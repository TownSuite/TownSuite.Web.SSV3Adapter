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
#if !DEBUG
        if (!string.IsNullOrWhiteSpace(jsonCached)) return (200, jsonCached ?? "");
#endif
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

            var requestSchema = new OpenApiSchema
            {
                Type = "object",
                Description = descriptionAttribute?.Description ?? "",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            var responseSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var prop in requestProperties)
            {
                HashSet<Type> processedTypes = new HashSet<Type>();
                var s = GetOpenApiSchema(prop.PropertyType, 0, processedTypes, swaggerDoc);
                if (s != null && !requestSchema.Properties.ContainsKey(prop.Name))
                {
                    requestSchema.Properties.Add(prop.Name, s);
                }
            }

            foreach (var prop in responseProperties)
            {
                HashSet<Type> processedTypes = new HashSet<Type>();
                if (!responseSchema.Properties.ContainsKey(prop.Name))
                {
                    var s = GetOpenApiSchema(prop.PropertyType, 0, processedTypes, swaggerDoc);
                    if (s != null && !responseSchema.Properties.ContainsKey(prop.Name))
                    {
                        responseSchema.Properties.Add(prop.Name, s);
                    }
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
                                            Reference = GetSchemaReference(theNamespace, endpointName, requestProp)
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
                                                Reference = GetSchemaReference(responseNamespace, responseName,
                                                    responseProp),
                                                Type = GetSchemaType(responseProp)
                                            }
                                        }
                                    }
                                },
                                ["299"] = new OpenApiResponse()
                                {
                                    Description = "Partial Success"
                                },
                                ["400"] = new OpenApiResponse()
                                {
                                    Description = "Bad Request"
                                },
                                ["401"] = new OpenApiResponse()
                                {
                                    Description = "Unauthorized"
                                },
                                ["403"] = new OpenApiResponse()
                                {
                                    Description = "Forbidden"
                                },
                                ["429"] = new OpenApiResponse()
                                {
                                    Description = "Too Many Requests"
                                },
                                ["500"] = new OpenApiResponse()
                                {
                                    Description = "Internal Server Error"
                                },
                                ["502"] = new OpenApiResponse()
                                {
                                    Description = "Bad Gateway"
                                },
                                ["504"] = new OpenApiResponse()
                                {
                                    Description = "Gateway Timeout"
                                }
                            }
                        }
                    }
                });
            }

            AddComponents(swaggerDoc, theNamespace, endpointName, requestSchema, requestProp);
            AddComponents(swaggerDoc, responseNamespace, responseName, responseSchema, responseProp);
        }

        var sb = new StringBuilder();
        swaggerDoc.SerializeAsV3(new OpenApiJsonWriter(new StringWriter(sb)));
        jsonCached = sb.ToString();
    }

    private static string GetSchemaType(Type type)
    {
        // If the type is a built-in type, return null
        if (type == typeof(string))
        {
            return "string";
        }
        else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return "number";
        }
        else if (type == typeof(int))
        {
            return "integer";
        }
        else if (type == typeof(bool))
        {
            return "boolean";
        }
        else if (type == typeof(DateTime))
        {
            return "string";
        }
        else if (type.IsPrimitive || type == typeof(object))
        {
            return "object";
        }
        
        return null;
    }

    private static void AddComponents(OpenApiDocument swaggerDoc, string? theNamespace, string endpointName,
        OpenApiSchema schema, Type type)
    {
        // If the type is a built-in type, return null
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) ||
            type == typeof(object))
        {
            return;
        }

        if (!swaggerDoc.Components.Schemas.ContainsKey($"{theNamespace}.{endpointName}"))
        {
            swaggerDoc.Components.Schemas.Add($"{theNamespace}.{endpointName}",
                schema);
        }
    }

    private static OpenApiReference GetSchemaReference(string? objectNamespace, string name, Type type)
    {
        // If the type is a built-in type, return null
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) ||
            type == typeof(object))
        {
            return null;
        }

        return new OpenApiReference()
        {
            Id = $"{objectNamespace}.{name}",
            Type = ReferenceType.Schema
        };
    }

    private OpenApiSchema GetOpenApiSchema(Type type, int level, HashSet<Type> processedTypes,
        OpenApiDocument swaggerDoc)
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

        return GetCustomObjectOpenApiSchema(type, level, processedTypes, swaggerDoc);
    }

    private OpenApiSchema GetCustomObjectOpenApiSchema(Type type, int level, HashSet<Type> processedTypes,
        OpenApiDocument swaggerDoc)
    {
        if (type == typeof(System.Object)) return null;

        // If the type is a custom object
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Reference = new OpenApiReference()
            {
                Id = $"{type.Namespace}.{type.Name}",
                Type = ReferenceType.Schema
            }
        };

        processedTypes.Add(type);
        foreach (var prop in type.GetProperties())
        {
            if (!schema.Properties.ContainsKey(prop.Name))
            {
                var s = GetOpenApiSchema(prop.PropertyType, level + 1, processedTypes, swaggerDoc);
                if (s != null)
                {
                    schema.Properties.Add(prop.Name, s);
                }
            }
        }

        if (!swaggerDoc.Components.Schemas.ContainsKey($"{type.Namespace}.{type.Name}"))
        {
            swaggerDoc.Components.Schemas.Add($"{type.Namespace}.{type.Name}",
                schema);
        }

        return schema;
    }
}