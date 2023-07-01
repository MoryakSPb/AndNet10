using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared.Models.Documentation.Info;

[JsonPolymorphic]
public record ProtocolInfo : DocInfo
{
    public ProtocolType ProtocolType { get; set; }
    public ImmutableList<int> Members { get; set; } = ImmutableList<int>.Empty;
}