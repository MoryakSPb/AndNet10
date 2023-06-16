using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;
using AndNet.Manager.Database.Models.Documentation.Report;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models;
using NpgsqlTypes;

namespace AndNet.Manager.Database.Models;

public record DbExpedition
{
    [JsonIgnore]
    public DbPlayer AccountablePlayer { get; set; } = null!;

    [JsonIgnore]
    public IList<DbPlayer> Members { get; set; } = null!;

    [JsonIgnore]
    public IList<DbDocumentReportExpedition> Reports { get; set; } = null!;

    [JsonIgnore]
    public IList<DbDocumentDecisionCouncilExpedition> CouncilPlayerDirectives { get; set; } = null!;

    public DateTime StartDate
    {
        get => During.LowerBound;
        set => During = new(value.ToUniversalTime(), During.UpperBound);
    }

    public DateTime EndDate
    {
        get => During.UpperBound;
        set => During = new(During.LowerBound, value.ToUniversalTime());
    }

    public NpgsqlRange<DateTime> During { get; set; }
    public int AccountablePlayerId { get; set; }
    public ulong? DiscordRoleId { get; set; }
    public int Id { get; set; }

    [JsonIgnore]
    public bool IsMarkedForDelete { get; set; }

    public uint Version { get; set; }

    public override int GetHashCode()
    {
        return Id;
    }

    public static explicit operator Expedition(DbExpedition expedition)
    {
        return new(expedition.Id,
            expedition.IsMarkedForDelete, expedition.Version, expedition.StartDate,
            expedition.EndDate, expedition.DiscordRoleId,
            expedition.AccountablePlayerId, !expedition.IsMarkedForDelete,
            expedition.Members?.Select(x => x.Id).ToArray());
    }
}