using Newtonsoft.Json;

public class BackwardsCompatStringConverter : JsonConverter<string>
{
    public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
    {
        writer.WriteValue(value ?? "");
    }

    public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var s = reader?.Value?.ToString();

        return s ?? "";
    }
}