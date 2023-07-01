using System.Collections.Immutable;
using System.Net;
using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Report;

[JsonPolymorphic]
[JsonDerivedType(typeof(ReportInfoBattle), "ОБ")]
public record ReportInfoBattle : ReportInfo
{
    public IPEndPoint? ServerEndPoint { get; set; }
    public ImmutableList<BattleCombatant> Combatants { get; set; } = ImmutableList<BattleCombatant>.Empty;

    public record BattleCombatant
    {
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? CommanderId { get; set; }
        public ImmutableList<int> Players { get; set; } = ImmutableList<int>.Empty;
        public ImmutableList<string> UnknownPlayers { get; set; } = ImmutableList<string>.Empty;
        public ImmutableList<string> Units { get; set; } = ImmutableList<string>.Empty;
        public ImmutableList<string> Casualties { get; set; } = ImmutableList<string>.Empty;
    }
}