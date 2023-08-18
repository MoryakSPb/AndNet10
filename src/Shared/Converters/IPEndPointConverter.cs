using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Converters;

public sealed class IPEndPointConverter : JsonConverter<IPEndPoint?>
{
    public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value is null ? null : IPEndPoint.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, IPEndPoint? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.ToString());
    }
}