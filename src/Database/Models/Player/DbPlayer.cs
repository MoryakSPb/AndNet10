using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Database.Models.Player;

public abstract record DbPlayer
{
    [JsonIgnore]
    public IList<DbPlayerContact> Contacts { get; set; } = null!;

    [JsonIgnore]
    public IList<DbAward> Awards { get; set; } = null!;

    [JsonIgnore]
    public IList<DbAward> IssuedAwards { get; set; } = null!;

    [JsonIgnore]
    public IList<DbExpedition> Expeditions { get; set; } = null!;

    [JsonIgnore]
    public IList<DbExpedition> AccountableExpeditions { get; set; } = null!;

    [JsonIgnore]
    public IList<DbDoc> CreatedDocuments { get; set; } = null!;

    [JsonIgnore]
    public int? IdentityId { get; set; }

    [JsonIgnore]
    public DbUser? Identity { get; set; }

    public int Id { get; set; }
    public PlayerStatus Status { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? RealName { get; set; }

    public ulong? DiscordId { get; set; }
    public ulong? SteamId { get; set; }

    [JsonIgnore]
    public DateTime DetectionDate { get; set; }

    public TimeZoneInfo? TimeZone { get; set; }

    public uint Version { get; set; }

    public override int GetHashCode()
    {
        return Id;
    }

    public override string ToString()
    {
        string result = Nickname;
        if (RealName is not null) result += $" ({RealName})";
        return result;
    }

    public static explicit operator Shared.Models.Player(DbPlayer player)
    {
        return new(player.Id, player.Version,
            player.Status, player.Nickname, player.ToString(), player.RealName, player.DiscordId, player.SteamId,
            player.DetectionDate, player.TimeZone?.Id);
    }
}