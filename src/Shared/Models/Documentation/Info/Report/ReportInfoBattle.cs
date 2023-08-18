using System.Collections.Immutable;
using System.Net;
using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Report;

[JsonPolymorphic]
[JsonDerivedType(typeof(ReportInfoBattle), "ОБ")]
public record ReportInfoBattle : ReportInfo
{
    public IPEndPoint? ServerEndPoint { get; set; }
    public List<BattleCombatant> Combatants { get; set; } = new List<BattleCombatant>();

    public record BattleCombatant
    {
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? CommanderId { get; set; }
        public List<int> Players { get; set; } = new List<int>();
        public List<string> UnknownPlayers { get; set; } = new List<string>();
        public List<string> Units { get; set; } = new List<string>();
        public List<string> Casualties { get; set; } = new List<string>();
    }
}