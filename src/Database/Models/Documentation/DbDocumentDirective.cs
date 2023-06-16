using AndNet.Manager.Database.Models.Documentation.Decisions.Directive;

namespace AndNet.Manager.Database.Models.Documentation;

public record DbDocumentDirective : DbDocument
{
    public override string Prefix { get; set; } = "Д";
    public DateTime? CancelDate { get; set; }

    public DateTime? AcceptanceDate { get; set; }

    public int? ReplacedById { get; set; }
    public DbDocumentDirective? ReplacedBy { get; set; }
    public IList<DbDocumentDirective> Previous { get; set; } = Array.Empty<DbDocumentDirective>();

    public IList<DbDocumentDecisionCouncilDirective> Directives { get; set; } =
        Array.Empty<DbDocumentDecisionCouncilDirective>();

    public IList<DbDocumentDecisionCouncilDirectiveChange> ChangeToDirectives { get; set; } =
        Array.Empty<DbDocumentDecisionCouncilDirectiveChange>();
}