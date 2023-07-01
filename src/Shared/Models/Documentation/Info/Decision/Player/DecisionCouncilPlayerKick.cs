using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

[JsonDerivedType(typeof(DecisionCouncilPlayerKick), "РCИИ")]
public record DecisionCouncilPlayerKick : DecisionCouncilPlayer
{
    public int SubstitutePlayerId { get; set; }
    public PlayerLeaveReason PlayerLeaveReason { get; set; }
}