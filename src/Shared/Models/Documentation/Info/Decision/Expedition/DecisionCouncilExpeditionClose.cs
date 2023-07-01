using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;

[JsonDerivedType(typeof(DecisionCouncilExpeditionClose), "РCЭЗ")]
public record DecisionCouncilExpeditionClose : DecisionCouncilExpedition
{
}