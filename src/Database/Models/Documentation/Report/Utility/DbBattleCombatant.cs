using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Report.Utility;

public record DbBattleCombatant
{
    public int BattleId { get; set; }
    public DbDocumentReportBattle Battle { get; set; } = null!;

    public int Number { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CommanderId { get; set; }
    public DbPlayer? Commander { get; set; }
    public IList<DbPlayer> Players { get; set; } = Array.Empty<DbPlayer>();
    public List<string> UnknownPlayers { get; set; } = new();

    public List<string> Units { get; } = new();
    public List<string> Casualties { get; } = new();
}