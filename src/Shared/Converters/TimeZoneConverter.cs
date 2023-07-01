using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Converters;

public class TimeZoneConverter : JsonConverter<TimeZoneInfo?>
{
    public override TimeZoneInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? id = reader.GetString();
        return id is null ? null : TimeZoneInfo.FindSystemTimeZoneById(id);
    }

    public override void Write(Utf8JsonWriter writer, TimeZoneInfo? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Id);
    }
}