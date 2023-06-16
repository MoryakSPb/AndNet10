using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndNet.Integration.Steam.Converters;

public class NullableUInt64FromStringConverter : JsonConverter<ulong?>
{
    public override ulong? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? raw = reader.GetString();
        return raw is null ? null : ulong.Parse(raw);
    }

    public override void Write(Utf8JsonWriter writer, ulong? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString("D"));
        else
            writer.WriteNullValue();
    }
}