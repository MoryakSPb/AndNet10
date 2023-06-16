using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpeditionRemovePlayer : DbDocumentDecisionCouncilExpedition
{
    public override string Prefix { get; set; } = "РСЭИ";
    public int PlayerId { get; set; }
    public DbPlayer Player { get; set; } = null!;

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        DbPlayer? player = Expedition.Members.FirstOrDefault(x => x.Id == PlayerId);
        if (player is not null) Expedition.Members.Remove(player);
    }
}