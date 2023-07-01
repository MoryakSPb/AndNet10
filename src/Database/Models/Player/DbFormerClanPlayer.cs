using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;

namespace AndNet.Manager.Database.Models.Player;

public record DbFormerClanPlayer : DbExternalPlayer
{
    public bool RestorationAvailable { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime LeaveDate { get; set; }
    public PlayerLeaveReason LeaveReason { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }

    public static explicit operator FormerClanPlayer(DbFormerClanPlayer player)
    {
        return new(player.Id, player.Version,
            player.Status, player.Nickname, player.ToString(), player.RealName, player.DiscordId, player.SteamId,
            player.DetectionDate, player.TimeZone?.Id, player.Relationship, player.JoinDate, player.LeaveDate,
            player.LeaveReason);
    }
}