using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndNet.Integration.Steam.Converters;

internal class IpEndPointConverter : JsonConverter<IPEndPoint>
{
    public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? raw = reader.GetString();
        return raw is null ? null : IPEndPoint.Parse(raw);
    }

    public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}