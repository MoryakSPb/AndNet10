using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Report;
using AndNet.Manager.Shared.Models.Election;

namespace AndNet.Manager.Client;

[JsonSerializable(typeof(Doc[]))]
[JsonSerializable(typeof(Dictionary<DateTime, PlayerStatisticsStatus>))]
[JsonSerializable(typeof(ImmutableList<ElectionCandidate>))]
[JsonSerializable(typeof(Player[]))]
[JsonSerializable(typeof(Doc))]
[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(Election))]
[JsonSerializable(typeof(Expedition[]))]
[JsonSerializable(typeof(GlobalStats))]
[JsonSerializable(typeof(Award[]))]
public partial class SerializationContext : JsonSerializerContext
{
    
}