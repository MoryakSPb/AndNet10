using System.Net;
using AndNet.Manager.Database.Models.Documentation.Report.Utility;

namespace AndNet.Manager.Database.Models.Documentation.Report;

public record DbDocumentReportBattle : DbDocumentReport
{
    public override string Prefix { get; set; } = "ОС";
    public IPEndPoint? ServerEndPoint { get; set; }
    public IList<DbBattleCombatant> Combatants { get; set; } = new List<DbBattleCombatant>();
}