using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Converters;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

[JsonDerivedType(typeof(DecisionCouncilPlayerAcceptApplication), "РCИП")]
public record DecisionCouncilPlayerAcceptApplication : DecisionCouncilPlayer
{
    public string Recommendation { get; set; } = string.Empty;

    public float? Hours { get; set; }

    [JsonConverter(typeof(TimeZoneConverter))]
    public TimeZoneInfo? TimeZone { get; set; } = TimeZoneInfo.Utc;

    public int? Age { get; set; }
}