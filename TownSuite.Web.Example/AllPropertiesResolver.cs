using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class AllPropertiesResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        property.Ignored = false;
        return property;
    }
}