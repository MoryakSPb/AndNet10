using System.Text.Json.Serialization;

namespace AndNet.Integration.Steam.Models;

public record SteamResult<T>
{
    [JsonPropertyName("response")]
    public T? Result { get; init; }
}