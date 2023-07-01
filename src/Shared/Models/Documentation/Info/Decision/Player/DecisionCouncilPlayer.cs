using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

[JsonDerivedType(typeof(DecisionCouncilPlayer), "РCИ")]
[JsonDerivedType(typeof(DecisionCouncilPlayerAcceptApplication), "РCИП")]
[JsonDerivedType(typeof(DecisionCouncilPlayerAwardSheet), "РCИН")]
[JsonDerivedType(typeof(DecisionCouncilPlayerChange), "РCИЗ")]
[JsonDerivedType(typeof(DecisionCouncilPlayerKick), "РCИИ")]
public record DecisionCouncilPlayer : DecisionCouncil, IPlayerId
{
    public enum PlayerAction
    {
        Generic,
        FromReserve,
        ToReserve,
        Rehabilitation
    }

    public PlayerAction Action { get; set; }
    public int PlayerId { get; set; }
}