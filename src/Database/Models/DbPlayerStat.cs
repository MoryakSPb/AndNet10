using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Database.Models;

public record DbPlayerStat
{
    public int PlayerId { get; set; }
    public DbPlayer Player { get; set; } = null!;
    public DateTime Date { get; set; }
    public PlayerStatisticsStatus Status { get; set; }
    public uint Version { get; set; }
}