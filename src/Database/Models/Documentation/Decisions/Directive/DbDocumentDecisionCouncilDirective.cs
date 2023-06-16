namespace AndNet.Manager.Database.Models.Documentation.Decisions.Directive;

public record DbDocumentDecisionCouncilDirective : DbDocumentDecisionCouncil
{
    public override string Prefix { get; set; } = "РСД";
    public int DirectiveId { get; set; }
    public DbDocumentDirective Directive { get; set; } = null!;
}