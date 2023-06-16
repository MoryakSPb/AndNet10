namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpedition : DbDocumentDecisionCouncil
{
    public override string Prefix { get; set; } = "РСЭ";
    public int ExpeditionId { get; set; }
    public DbExpedition Expedition { get; set; } = null!;
}