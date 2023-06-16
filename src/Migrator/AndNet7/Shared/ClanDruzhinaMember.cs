using System.Text.Json.Serialization;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared;

public class ClanDruzhinaMember
{
    [JsonIgnore]
    public int DruzhinaId { get; set; }

    [JsonIgnore]
    public virtual ClanDruzhina Druzhina { get; set; } = null!;

    [JsonIgnore]
    public int MemberId { get; set; }

    public virtual ClanMember Member { get; set; } = null!;
    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public ClanDruzhinaPositionEnum Position { get; set; }
}