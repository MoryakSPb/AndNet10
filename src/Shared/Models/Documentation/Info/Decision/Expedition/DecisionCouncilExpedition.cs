using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;

[JsonDerivedType(typeof(DecisionCouncilExpedition), "РCЭ")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionClose), "РCЭЗ")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionCreate), "РCЭС")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionPlayer), "РCЭИ")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionProlongation), "РCЭП")]
public record DecisionCouncilExpedition : DecisionCouncil, IExpeditionId
{
    public int ExpeditionId { get; set; }
}