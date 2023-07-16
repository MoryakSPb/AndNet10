using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;

[JsonDerivedType(typeof(DecisionCouncilExpeditionCreate), "РCЭС")]
public record DecisionCouncilExpeditionCreate : DecisionCouncil, IExpeditionId
{
    public int? ExpeditionId { get; set; }
    public int AccountablePlayerId { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    public IReadOnlyCollection<int> Members { get; set; } = ImmutableList<int>.Empty;
    int IExpeditionId.ExpeditionId => ExpeditionId.GetValueOrDefault();
}