using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Player;

public record DbDocumentDecisionCouncilPlayerFromReserve : DbDocumentDecisionCouncilPlayer
{
    public override string Prefix { get; set; } = "РСИВ";

    protected override async Task ExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        await base.ExecuteAsync(signer, context).ConfigureAwait(false);
        if (Player is DbClanPlayer clanPlayer) clanPlayer.OnReserve = false;
    }
}