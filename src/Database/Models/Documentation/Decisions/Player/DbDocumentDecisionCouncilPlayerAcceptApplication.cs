using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Player;

public record DbDocumentDecisionCouncilPlayerAcceptApplication : DbDocumentDecisionCouncilPlayer
{
    public override string Prefix { get; set; } = "РСИО";
    public string Recommendation { get; set; } = string.Empty;
    public float? Hours { get; set; }
    public TimeZoneInfo? TimeZone { get; set; } = TimeZoneInfo.Utc;
    public int? Age { get; set; }

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        DbClanPlayer newPlayer = new()
        {
            Rank = PlayerRank.Neophyte,
            DiscordId = Player.DiscordId,
            DetectionDate = Player.DetectionDate,
            JoinDate = DateTime.UtcNow,
            Nickname = Player.Nickname,
            Status = PlayerStatus.Member,
            OnReserve = false,
            RealName = Player.RealName,
            SteamId = Player.SteamId,
            Id = PlayerId
        };
        context.Update(newPlayer).State = EntityState.Modified;
        newPlayer.CalcPlayer(context.Awards);
    }
}