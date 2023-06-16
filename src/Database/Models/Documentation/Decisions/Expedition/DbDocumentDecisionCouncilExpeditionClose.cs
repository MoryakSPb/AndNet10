using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpeditionClose : DbDocumentDecisionCouncilExpedition
{
    public override string Prefix { get; set; } = "РСЭЗ";

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Expedition.EndDate = DateTime.UtcNow;
    }
}