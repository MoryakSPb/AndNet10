using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Player;

public record DbDocumentDecisionCouncilPlayerAwardSheet : DbDocumentDecisionCouncilPlayer
{
    public override string Prefix { get; set; } = "РСИН";
    public AwardType AwardType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DbAward? Award { get; set; }

    protected override async Task ExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        await base.ExecuteAsync(signer, context).ConfigureAwait(false);
        EntityEntry<DbAward> entityState = await context.Awards.AddAsync(new()
        {
            AwardSheet = this,
            AutomationId = null,
            AwardType = AwardType,
            PlayerId = PlayerId,
            Player = Player,
            IsMarkedForDelete = false,
            IssueDate = DateTime.UtcNow,
            Issuer = signer,
            IssuerId = signer.Id,
            Description = Description
        }).ConfigureAwait(false);
        Award = entityState.Entity;
        if (Player is DbClanPlayer clanPlayer) clanPlayer.CalcPlayer(context.Awards);
    }
}