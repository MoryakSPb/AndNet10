using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Directive;

public record DbDocumentDecisionCouncilDirectiveCancel : DbDocumentDecisionCouncilDirective
{
    public override string Prefix { get; set; } = "РСДО";

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Directive.CancelDate = DateTime.UtcNow;
    }
}