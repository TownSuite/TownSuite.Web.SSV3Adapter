
using Newtonsoft.Json;

public class BackwardsCompatStringConverter : JsonConverter<string>
{
    public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString() ?? "");
    }

    public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string s = reader?.Value?.ToString();

        return s ?? "";
    }
}
