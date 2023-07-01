using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;

[JsonDerivedType(typeof(DecisionCouncilDirective), "РCД")]
[JsonDerivedType(typeof(DecisionCouncilDirectiveChange), "РCДЗ")]
public record DecisionCouncilDirective : DecisionCouncil
{
    public enum DirectiveAction : byte
    {
        Generic,
        Accept,
        Cancel
    }

    public int DirectiveId { get; set; }
    public DirectiveAction Action { get; set; }
}