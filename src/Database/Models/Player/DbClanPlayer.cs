using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Database.Models.Player;

public record DbClanPlayer : DbPlayer
{
    public DateTime JoinDate { get; set; }
    public PlayerRank Rank { get; set; }
    public double Score { get; set; }
    public bool OnReserve { get; set; }

    public void CalcPlayer(DbSet<DbAward> awards)
    {
        Score = awards
            .Where(x => x.PlayerId == Id)
            .Sum(x =>
                (double)x.AwardType * Math.Pow(2d, -Math.Truncate((DateTime.Now - x.IssueDate).TotalDays) / 365.25));
        if (Rank < PlayerRank.Advisor)
            Rank = RankRules.MinimalScores.Where(x => x.Key <= Score).MaxBy(x => x.Key).Value;
    }

    public override string ToString()
    {
        return $"[{RankRules.Icons[Rank]}] {base.ToString()}";
    }

    public static explicit operator ClanPlayer(DbClanPlayer player)
    {
        return new(player.Id, player.Version, player.Status, player.Nickname, player.ToString(), player.RealName,
            player.DiscordId, player.SteamId, player.JoinDate, player.JoinDate, player.Rank, player.Score,
            player.OnReserve);
    }
}