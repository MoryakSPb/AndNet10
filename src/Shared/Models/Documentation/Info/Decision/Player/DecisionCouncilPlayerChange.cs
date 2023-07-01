using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

[JsonDerivedType(typeof(DecisionCouncilPlayerChange), "РCИЗ")]
public record DecisionCouncilPlayerChange : DecisionCouncilPlayer
{
    public enum PlayerChangeProperty
    {
        Unknown,
        Nickname,
        RealName
    }

    public string? NewValue { get; set; }
    public PlayerChangeProperty Property { get; set; }
}