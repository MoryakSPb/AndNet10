using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;

[JsonDerivedType(typeof(DecisionCouncilDirectiveChange), "РCДЗ")]
public record DecisionCouncilDirectiveChange : DecisionCouncilDirective
{
    public int NewDirectiveId { get; set; }
}