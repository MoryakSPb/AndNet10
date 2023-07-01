using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;

[JsonDerivedType(typeof(DecisionCouncilExpeditionPlayer), "РCЭИ")]
public record DecisionCouncilExpeditionPlayer : DecisionCouncilExpedition, IPlayerId
{
    public enum ExpeditionPlayerAction : byte
    {
        Unknown,
        Add,
        Remove,
        ChangeCommander
    }

    public ExpeditionPlayerAction Action { get; set; }
    public int PlayerId { get; set; }
}