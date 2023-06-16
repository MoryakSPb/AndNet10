using System.Text.Json.Serialization;

namespace AndNet.Integration.Steam.Models;

public record PlayerActivityResultCollection
{
    [JsonPropertyName("players")]
    public PlayerActivityResultNode[]? Players { get; init; }
}