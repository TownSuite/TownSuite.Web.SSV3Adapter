using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TownSuite.Web.SSV3Facade
{
    internal class Swagger
    {
        private readonly ServiceStackV3FacadeOptions _options;
        private readonly SsHelper _ssHelper;
        private readonly string _description;
        private readonly string _title;
        private readonly string _version;

        public Swagger(ServiceStackV3FacadeOptions options,
            IServiceProvider serviceProvider, string description,
            string title, string version)
        {
            _options = options;
            _ssHelper = new SsHelper(options, serviceProvider);
            _description = description;
            _title = title;
            _version = version;
        }


        private static string? jsonCached = null;

        public async Task<(int statusCode, string json)> Generate(string host)
        {
            if (!string.IsNullOrWhiteSpace(jsonCached))
            {
                return (200, jsonCached ?? "");
            }

            await PreGenerateJson(host);

            return (200, jsonCached ?? "");
        }

        RootInfo swagRoot;

        /// <summary>
        /// Used to pre
        /// </summary>
        private async Task PreGenerateJson(string host)
        {
            var serviceInfo = _ssHelper.GetAllServices();

            swagRoot = new RootInfo()
            {
                Swagger = "2.0",
                Info = new Info()
                {
                    Description = _description,
                    Title = _title,
                    Version = _version
                },
                Host = host,
                BasePath = _options.RoutePath,
                Schemes = new string[] {"https"},
                Paths = new SortedDictionary<string, object>(),
                Definitions = new SortedDictionary<string, object>()
            };

            var expandoPath = swagRoot.Paths;
            foreach (var service in serviceInfo)
            {
                var data = CanAddAttribute(service.Key.Name, service.Value.Service);

                var descriptionAttribute = await _ssHelper.GetAttributeAsync<DescriptionAttribute>(
                    service.Value.Service
                );

                data.PostData = new Post()
                {
                    Description = descriptionAttribute?.Description ?? "",
                    Produces = new string[] {"application/json"},
                    Responses = new Responses()
                    {
                        The200 = new The200()
                        {
                            Description = "OK",
                            Schema = new Schema()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, object>()
                            }
                        }
                    },
                    Consumes = new string[] {"application/json"},
                    Summary = "",
                    Parameters = new RequestBody2[]
                    {
                        new RequestBody2()
                        {
                            In = "body",
                            Description = descriptionAttribute?.Description ?? "",
                            Name = service.Value.DtoType.Name,
                            Schema = new Schema()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, object>()
                            }
                        }
                    },
                };

                IDictionary<string, object> requestExpandoBody = data.PostData.Parameters[0].Schema.Properties;
                IDictionary<string, object> responseExpandoBody = data.PostData.Responses.The200.Schema.Properties;


                var requestProperties = service.Value.DtoType.GetProperties();
                PropertyInfo[] responseProperties;
                if (SsHelper.IsAsyncMethod(service.Value.Method))
                {
                    responseProperties = service.Value.Method.ReturnType
                        .GetProperties().FirstOrDefault()
                        .PropertyType.GetProperties();
                }
                else
                {
                    responseProperties = service.Value.Method.ReturnType.GetProperties();
                }

                ExtractRequestBody(requestExpandoBody,
                    requestProperties, 0);
                ExtractRequestBody(responseExpandoBody,
                    responseProperties, 0);

                try
                {
                    if (!expandoPath.ContainsKey("/" + service.Value.DtoType.Name))
                    {
                        expandoPath.Add("/" + service.Value.DtoType.Name, data);
                    }
                    else
                    {
                        expandoPath.Add("/" + service.Value.DtoType.FullName, data);
                    }
                }
                catch (ArgumentException)
                {
                }
            }


            jsonCached = Newtonsoft.Json.JsonConvert.SerializeObject(swagRoot);
        }


        private void ExtractRequestBody(IDictionary<string, object> expandoBody,
            PropertyInfo[] requestProperties, int level)
        {
            //  Console.WriteLine(prop?.PropertyType?.ToString());
            //  Console.WriteLine($"{prop?.Name}|{paramType}|{basetype}".PadLeft(level * 4, '.'));

            //if (level > 5)
            //{
            //    Console.WriteLine("bad");
            //}

            foreach (var prop in requestProperties)
            {
                string paramType = prop?.PropertyType.Name.ToLower();
                string basetype = prop?.PropertyType?.BaseType?.Name.ToLower();
                string namespaceType = prop?.PropertyType?.Namespace?.ToLower();

                switch (basetype)
                {
                    case "enum":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new
                            {
                                type = "string",
                                @enum = System.Enum.GetNames(prop.PropertyType)
                            }));
                        continue;
                }


                bool isNullable = false;
                if (paramType == "nullable`1")
                {
                    isNullable = true;
                    paramType = prop?.PropertyType?.GenericTypeArguments.FirstOrDefault().Name.ToLower();
                }


                switch (paramType)
                {
                    // nullable: true

                    case "byte[]":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"type", "string"},
                                {"format", "binary"},
                                {"x-nullable", isNullable.ToString().ToLower()}
                            }
                        ));
                        break;
                    case "string[]":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"type", "array"},
                                {"format", "string"},
                                {"x-nullable", isNullable.ToString().ToLower()}
                            }
                        ));
                        break;
                    case "datetime":
                    case "date":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"type", "string"},
                                {"format", "datetime"},
                                {"x-nullable", isNullable.ToString().ToLower()}
                            }
                        ));
                        break;
                    case "guid":
                    case "char":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"type", "string"},
                                {"format", "string"},
                                {"x-nullable", isNullable.ToString().ToLower()}
                            }
                        ));
                        break;
                    case "decimal":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"type", "number"},
                                {"format", "double"},
                                {"x-nullable", isNullable.ToString().ToLower()}
                            }
                        ));
                        break;
                    case "int32":
                    case "int64":
                    case "double":
                    case "float":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"type", "number"},
                                {"format", paramType},
                                {"x-nullable", isNullable.ToString().ToLower()}
                            }));
                        break;
                    case "reporttype":
                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new
                            {
                                type = "string",
                                @enum = System.Enum.GetNames(prop.PropertyType)
                            }));
                        break;
                    case "taskfactory`1":
                        return;
                    case "ilist`1":
                    case "icollection`1":
                    case "ienumerable`1":
                    case "list`1":

                        if (string.Equals(prop.DeclaringType.FullName,
                                prop.PropertyType.GenericTypeArguments.FirstOrDefault().FullName,
                                StringComparison.InvariantCultureIgnoreCase)
                            || prop.PropertyType.GenericTypeArguments.FirstOrDefault().FullName ==
                            prop.ReflectedType.FullName
                           )
                        {
                            continue;
                        }


                        var dict = new Dictionary<string, object>();
                        var kvp = new KeyValuePair<string, object>(prop?.Name,
                            new
                            {
                                type = "object",
                                properties = dict
                            });

                        ExtractRequestBody(dict,
                            prop.PropertyType.GenericTypeArguments.FirstOrDefault().GetProperties(),
                            level + 1);
                        AddIfNotPresent(expandoBody, kvp);
                        break;

                    default:


                        if (namespaceType.StartsWith("system"))
                        {
                            AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                                new
                                {
                                    type = paramType
                                }));
                            break;
                        }


                        var dict2 = new Dictionary<string, object>();
                        var kvp2 = new KeyValuePair<string, object>(prop?.PropertyType?.Name,
                            new
                            {
                                type = "object",
                                properties = dict2
                            });

                        // Set a ref instead of inline definition.

                        /*
                         * 
                          "address": {
                              "$ref": "#/definitions/Address"
                            },
                         * 
                         */

                        AddIfNotPresent(expandoBody, new KeyValuePair<string, object>(prop?.Name,
                            new Dictionary<string, string>()
                            {
                                {"$ref", $"#/definitions/{prop?.PropertyType?.Name}"},
                            }
                        ));

                        if (string.Equals(prop.PropertyType.FullName,
                                prop.DeclaringType?.FullName))
                        {
                            AddIfNotPresent(swagRoot.Definitions, kvp2);
                            continue;
                        }

                        ExtractRequestBody(dict2,
                            prop.PropertyType.GetProperties(),
                            level + 1);
                        if (!swagRoot.Definitions.ContainsKey(kvp2.Key))
                        {
                            AddIfNotPresent(swagRoot.Definitions, kvp2);
                        }

                        break;
                }
            }
        }

        private void AddIfNotPresent(IDictionary<string, object> expandoBody,
            KeyValuePair<string, object> kvp)
        {
            if (expandoBody.ContainsKey(kvp.Key)) return;

            expandoBody.Add(kvp);
        }


        Type type = null;
        AssemblyName aName = null;
        AssemblyBuilder ab = null;
        ModuleBuilder mb = null;
        TypeBuilder tb = null;

        private ServiceEndPoint CanAddAttribute(string path, Type service)
        {
            // See https://stackoverflow.com/questions/14663763/how-to-add-an-attribute-to-a-property-at-runtime

            if (type == null)
            {
                type = typeof(ServiceEndPoint);
            }

            if (aName == null)
            {
                aName = new System.Reflection.AssemblyName(service.Assembly.FullName);
            }

            if (ab == null)
            {
                ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()),
                    AssemblyBuilderAccess.Run);
            }

            if (mb == null)
            {
                mb = ab.DefineDynamicModule(aName.Name);
            }

            if (tb == null)
            {
                tb = mb.DefineType(type.Name + "Proxy",
                    TypeAttributes.Public, type);
            }

            var attrCtorParams = new Type[] {typeof(string)};
            var attrCtorInfo = typeof(JsonPropertyAttribute).GetConstructor(attrCtorParams);
            var attrBuilder = new CustomAttributeBuilder(attrCtorInfo, new object[] {path});
            tb.SetCustomAttribute(attrBuilder);

            var newType = tb.CreateType();
            var instance = Activator.CreateInstance(newType) as ServiceEndPoint;

            return instance;
        }
    }
}