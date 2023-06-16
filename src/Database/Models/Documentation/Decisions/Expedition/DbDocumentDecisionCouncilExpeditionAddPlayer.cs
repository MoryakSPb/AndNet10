using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpeditionAddPlayer : DbDocumentDecisionCouncilExpedition
{
    public override string Prefix { get; set; } = "РСЭД";
    public int PlayerId { get; set; }
    public DbPlayer Player { get; set; } = null!;

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Expedition.Members.Add(Player);
    }
}