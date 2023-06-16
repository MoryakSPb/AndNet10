using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Directive;

public record DbDocumentDecisionCouncilDirectiveAccept : DbDocumentDecisionCouncilDirective
{
    public override string Prefix { get; set; } = "РСДП";

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Directive.AcceptanceDate = DateTime.UtcNow;
    }
}