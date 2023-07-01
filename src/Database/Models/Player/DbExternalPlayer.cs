using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;

namespace AndNet.Manager.Database.Models.Player;

public record DbExternalPlayer : DbPlayer
{
    public PlayerRelationship Relationship { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }

    public static explicit operator ExternalPlayer(DbExternalPlayer player)
    {
        return new(player.Id,
            player.Version, player.Status, player.Nickname, player.ToString(), player.RealName, player.DiscordId,
            player.SteamId,
            player.DetectionDate, player.TimeZone?.Id, player.Relationship);
    }
}