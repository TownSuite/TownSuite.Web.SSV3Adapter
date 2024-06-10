using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.OpenApi.Any;
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
    //    var sortedServiceInfo =
    //        serviceInfo.ToImmutableSortedDictionary(t => t.Key.Name, t => t.Value);
        
    // Create a new empty ImmutableSortedDictionary
    var sortedServiceInfo = ImmutableSortedDictionary<string, (Type Service, MethodInfo Method, Type DtoType)>.Empty;

    foreach (var service in serviceInfo)
    {
        // Check if the key already exists in the dictionary
        if (!sortedServiceInfo.ContainsKey(service.Key.Name))
        {
            // If the key does not exist, add the new element
            sortedServiceInfo = sortedServiceInfo.Add(service.Key.Name, service.Value);
        }
        else
        {
            // FIXME: add logging
            // If the key exists, decide whether to update the existing value or ignore the new one
            // To update the existing value, uncomment the following line:
            // sortedServiceInfo = sortedServiceInfo.SetItem(service.Key.Name, service.Value);
        }
    }
    
        var swaggerDoc = new OpenApiDocument();
        swaggerDoc.Info = new OpenApiInfo()
        {
            Title = _title,
            Version = _version,
            Description = _description
        };
        swaggerDoc.Components = new OpenApiComponents();
        swaggerDoc.Components.Schemas = new SortedDictionary<string, OpenApiSchema>();
        swaggerDoc.Paths = new OpenApiPaths();
        
        foreach (var service in sortedServiceInfo)
        {
            //  var service = serviceInfo[key];
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
                Properties = new SortedDictionary<string, OpenApiSchema>()
            };

            var responseSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new SortedDictionary<string, OpenApiSchema>()
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
                    Operations = new SortedDictionary<OperationType, OpenApiOperation>()
                    {
                        [OperationType.Post] = new OpenApiOperation()
                        {
                            RequestBody = new OpenApiRequestBody()
                            {
                                Content = new SortedDictionary<string, OpenApiMediaType>()
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
                                    Content = new SortedDictionary<string, OpenApiMediaType>()
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

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
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

        if(type ==typeof(int[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "integer" } };
        if(type ==typeof(string[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } };
        if(type ==typeof(decimal[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "number" } };
        if(type ==typeof(double[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "number" } };
        if(type ==typeof(float[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "number" } };
        if(type ==typeof(DateTime[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } };
        if(type ==typeof(bool[])) return new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "boolean" } };
        if(type == typeof(byte[])) return new OpenApiSchema { Type = "string", Format = "byte" };
        if (type == typeof(System.Guid)) return new OpenApiSchema { Type = "string", Format = "uuid" };
        if (type == typeof(System.DateTimeOffset)) return new OpenApiSchema { Type = "string", Format = "date-time" };
        if(type == typeof(System.Char)) return new OpenApiSchema { Type = "string", Format = "char" };
        
        return GetCustomObjectOpenApiSchema(type, level, processedTypes, swaggerDoc);
    }

    private OpenApiSchema GetCustomObjectOpenApiSchema(Type type, int level, HashSet<Type> processedTypes,
        OpenApiDocument swaggerDoc)
    {
        if (type == typeof(System.Object)) return null;
        
        
        // If the type is a TimeZoneInfo
        if (type == typeof(TimeZoneInfo))
        {
            return new OpenApiSchema
            {
                Type = "string"
            };
        }
        
        // If the type is a TimeSpan
        if (type == typeof(TimeSpan))
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "duration"
            };
        }
        
        // If the type is a Nullable<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // Get the underlying type of the nullable type
            var underlyingType = Nullable.GetUnderlyingType(type);
            // Return the OpenAPI schema of the underlying type
            return GetOpenApiSchema(underlyingType, level + 1, processedTypes, swaggerDoc);
        }
        
        // If the type is a KeyValuePair
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var keyValuePairSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new SortedDictionary<string, OpenApiSchema>()
            };

            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];

            var keySchema = GetOpenApiSchema(keyType, level + 1, processedTypes, swaggerDoc);
            if (keySchema != null)
            {
                keyValuePairSchema.Properties.Add("Key", keySchema);
            }

            var valueSchema = GetOpenApiSchema(valueType, level + 1, processedTypes, swaggerDoc);
            if (valueSchema != null)
            {
                keyValuePairSchema.Properties.Add("Value", valueSchema);
            }

            return keyValuePairSchema;
        }
        
        // If the type is a Tuple
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<>))
        {
            var tupleSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new SortedDictionary<string, OpenApiSchema>()
            };

            var tupleArguments = type.GetGenericArguments();
            for (int i = 0; i < tupleArguments.Length; i++)
            {
                var s = GetOpenApiSchema(tupleArguments[i], level + 1, processedTypes, swaggerDoc);
                if (s != null)
                {
                    tupleSchema.Properties.Add($"Item{i + 1}", s);
                }
            }

            return tupleSchema;
        }
        
        // If the type is a Tuple<T1, T2>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>))
        {
            var tupleSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new SortedDictionary<string, OpenApiSchema>()
            };

            var tupleArguments = type.GetGenericArguments();
            for (int i = 0; i < tupleArguments.Length; i++)
            {
                var s = GetOpenApiSchema(tupleArguments[i], level + 1, processedTypes, swaggerDoc);
                if (s != null)
                {
                    tupleSchema.Properties.Add($"Item{i + 1}", s);
                }
            }

            return tupleSchema;
        }
        
        // If the type is an array of DayOfWeek
        if (type.IsArray && type.GetElementType() == typeof(DayOfWeek))
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Type = "string",
                    Enum = Enum.GetNames(typeof(DayOfWeek)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
                }
            };
        }
        
        
        // If the type is a List
        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) 
                                   || type.GetGenericTypeDefinition() == typeof(IEnumerable<>) 
                                   || type.GetGenericTypeDefinition() == typeof(IList<>)
                                   || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                                   ))
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GetOpenApiSchema(type.GetGenericArguments()[0], level + 1, processedTypes, swaggerDoc)
            };
        }

        // If the type is a Dictionary or IDictionary
        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
        {
            return new OpenApiSchema
            {
                Type = "object",
                AdditionalProperties = GetOpenApiSchema(type.GetGenericArguments()[1], level + 1, processedTypes, swaggerDoc)
            };
        }
        
        // If the type is a custom object
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new SortedDictionary<string, OpenApiSchema>(),
            Reference = new OpenApiReference()
            {
                Id = $"{type.Namespace}.{type.Name}",
                Type = ReferenceType.Schema,
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