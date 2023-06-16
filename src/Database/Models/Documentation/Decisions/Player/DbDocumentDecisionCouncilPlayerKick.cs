using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Player;

public record DbDocumentDecisionCouncilPlayerKick : DbDocumentDecisionCouncilPlayer
{
    public override string Prefix { get; set; } = "РСИИ";
    public PlayerLeaveReason PlayerLeaveReason { get; set; }

    protected override async Task ExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        await base.ExecuteAsync(signer, context).ConfigureAwait(false);
        DbFormerClanPlayer newPlayer = new()
        {
            DiscordId = Player.DiscordId,
            DetectionDate = Player.DetectionDate,
            JoinDate = DateTime.UtcNow,
            Nickname = Player.Nickname,
            Status = PlayerStatus.Former,
            RealName = Player.RealName,
            SteamId = Player.SteamId,
            Id = PlayerId,
            LeaveDate = DateTime.UtcNow,
            RestorationAvailable = PlayerLeaveReason < PlayerLeaveReason.Exclude,
            Relationship = PlayerRelationship.Unknown,
            LeaveReason = PlayerLeaveReason
        };
        context.Update(newPlayer).State = EntityState.Modified;
        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}