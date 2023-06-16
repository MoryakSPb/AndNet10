using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Player;

public record DbDocumentDecisionCouncilPlayerRehabilitation : DbDocumentDecisionCouncilPlayer
{
    public override string Prefix { get; set; } = "РСИП";

    protected override async Task ExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        await base.ExecuteAsync(signer, context).ConfigureAwait(false);
        if (Player is DbFormerClanPlayer formerClanPlayer) formerClanPlayer.RestorationAvailable = true;
    }
}