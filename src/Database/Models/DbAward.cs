using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;

namespace AndNet.Manager.Database.Models;

public record DbAward
{
    [JsonIgnore]
    public DbPlayer Player { get; set; } = null!;

    [JsonIgnore]
    public DbPlayer? Issuer { get; set; } = null!;

    public DateTime IssueDate { get; set; }
    public int? AutomationId { get; set; }
    public int Id { get; set; }
    public AwardType AwardType { get; set; }
    public int PlayerId { get; set; }
    public int? IssuerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public uint Version { get; set; }

    [JsonIgnore]
    public bool IsMarkedForDelete { get; set; }

    public int AwardSheetId { get; set; }
    public DbDoc AwardSheet { get; set; } = null!;

    public override int GetHashCode()
    {
        return Id;
    }

    public static explicit operator Award(DbAward dbAward)
    {
        return new(dbAward.Id, dbAward.IsMarkedForDelete,
            dbAward.Version, dbAward.AwardType, dbAward.IssueDate, dbAward.PlayerId, dbAward.IssuerId,
            dbAward.Description, dbAward.AwardSheetId,
            (double)dbAward.AwardType
            * Math.Pow(2d, -Math.Truncate((DateTime.UtcNow - dbAward.IssueDate).TotalDays) / 365.25));
    }
}