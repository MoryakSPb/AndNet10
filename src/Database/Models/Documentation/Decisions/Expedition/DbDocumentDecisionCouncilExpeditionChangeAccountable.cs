using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpeditionChangeAccountable : DbDocumentDecisionCouncilExpedition
{
    public override string Prefix { get; set; } = "РСЭК";
    public int AccountablePlayerId { get; set; }
    public DbPlayer AccountablePlayer { get; set; } = null!;

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Expedition.AccountablePlayerId = AccountablePlayerId;
        Expedition.AccountablePlayer = AccountablePlayer;
    }
}