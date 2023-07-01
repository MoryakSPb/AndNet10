using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

[JsonDerivedType(typeof(DecisionCouncilPlayerAwardSheet), "РCИН")]
public record DecisionCouncilPlayerAwardSheet : DecisionCouncilPlayer
{
    public AwardType AwardType { get; set; }
    public int? AutomationId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? PredeterminedIssueDate { get; set; }
}