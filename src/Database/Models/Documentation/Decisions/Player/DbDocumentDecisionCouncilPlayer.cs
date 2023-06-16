using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Player;

public record DbDocumentDecisionCouncilPlayer : DbDocumentDecisionCouncil
{
    public override string Prefix { get; set; } = "РСИ";
    public int PlayerId { get; set; }
    public DbPlayer Player { get; set; } = null!;
}