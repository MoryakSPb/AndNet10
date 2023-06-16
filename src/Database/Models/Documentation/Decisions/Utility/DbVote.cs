using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Utility;

public record DbVote
{
    public int DecisionId { get; set; }
    public DbDocumentDecision Decision { get; set; } = null!;
    public int PlayerId { get; set; }
    public DbPlayer Player { get; set; } = null!;
    public DateTime Date { get; set; }
    public VoteType VoteType { get; set; } = VoteType.None;
}