using System.Text.Json.Serialization;
using AndNet.Integration.Steam.Converters;

namespace AndNet.Integration.Steam.Models;

public record ResolveSteamUrlResult
{
    [JsonPropertyName("steamid")]
    [JsonConverter(typeof(NullableUInt64FromStringConverter))]
    public ulong? SteamId { get; init; }
}