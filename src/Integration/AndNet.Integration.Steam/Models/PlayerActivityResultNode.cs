using System.Net;
using System.Text.Json.Serialization;
using AndNet.Integration.Steam.Converters;

namespace AndNet.Integration.Steam.Models;

public readonly record struct PlayerActivityResultNode
{
    private const string _SPACE_ENGINEERS_APP_ID = "244850";

    [JsonPropertyName("steamid")]
    [JsonConverter(typeof(UInt64FromStringConverter))]
    public ulong SteamId { get; init; }

    [JsonPropertyName("personaname")]
    public string? Nickname { get; init; }

    [JsonPropertyName("realname")]
    public string? RealName { get; init; }

    [JsonPropertyName("gameserverip")]
    [JsonConverter(typeof(IpEndPointConverter))]
    public IPEndPoint? GameServerId { get; init; }

    [JsonPropertyName("gameserversteamid")]
    [JsonConverter(typeof(NullableUInt64FromStringConverter))]
    public ulong? GameServerSteamId { get; init; }

    [JsonPropertyName("lobbysteamid")]
    [JsonConverter(typeof(NullableUInt64FromStringConverter))]
    public ulong? LobbySteamId { get; init; }

    [JsonPropertyName("gameid")]
    public string? GameId { get; init; }

    [JsonIgnore]
    public bool InSpaceEngineers => string.Equals(GameId, _SPACE_ENGINEERS_APP_ID, StringComparison.Ordinal);
}