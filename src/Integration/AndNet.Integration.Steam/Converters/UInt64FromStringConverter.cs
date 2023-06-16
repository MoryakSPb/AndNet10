using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndNet.Integration.Steam.Converters;

internal class UInt64FromStringConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? raw = reader.GetString();
        return raw is null ? throw new() : ulong.Parse(raw);
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("D"));
    }
}