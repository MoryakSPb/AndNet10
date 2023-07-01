using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;

[JsonDerivedType(typeof(DecisionCouncilExpeditionProlongation), "РCЭП")]
public record DecisionCouncilExpeditionProlongation : DecisionCouncilExpedition
{
    public TimeSpan ProlongationTime { get; set; }
}