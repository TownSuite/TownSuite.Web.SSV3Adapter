using System;
using Newtonsoft.Json;

namespace TownSuite.Web.SSV3Facade
{
    public partial class RootInfo
    {
        [JsonProperty("swagger")]
        public string Swagger { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("basePath")]
        public string BasePath { get; set; }

        [JsonProperty("schemes")]
        public string[] Schemes { get; set; }

        [JsonProperty("paths")]
        public IDictionary<string, object> Paths { get; set; }

        [JsonProperty("definitions")]
        public IDictionary<string, object> Definitions { get; set; }
    }


    public partial class Info
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class ServiceEndPoint
    {
        [JsonProperty("post")]
        public Post PostData { get; set; }
    }

    public partial class Post
    {
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("parameters")]
        public RequestBody2[] Parameters { get; set; }

        [JsonProperty("consumes")]
        public string[] Consumes { get; set; }


        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("produces")]
        public string[] Produces { get; set; }

        [JsonProperty("responses")]
        public Responses Responses { get; set; }
    }

    public partial class Responses
    {
        [JsonProperty("200")]
        public The200 The200 { get; set; }
    }

    public partial class The200
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("schema")]
        public Schema Schema { get; set; }
    }


    public partial class RequestBody2
    {
        [JsonProperty("in")]
        public string In { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("schema")]
        public Schema Schema { get; set; }
    }

    public partial class RequestBody
    {
        [JsonProperty("content")]
        public Content Content { get; set; }
    }

    public partial class Content
    {
        [JsonProperty("application/json")]
        public ApplicationJson ApplicationJson { get; set; }
    }

    public partial class ApplicationJson
    {
        [JsonProperty("schema")]
        public Schema Schema { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public IDictionary<string, object> Properties { get; set; }

    }
}

