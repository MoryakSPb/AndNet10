using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Database.Models.Documentation;

public record DbDocumentProtocol : DbDocument
{
    public override string Prefix { get; set; } = "П";
    public ProtocolType ProtocolType { get; set; }
    public IList<DbPlayer> Members { get; set; } = Array.Empty<DbPlayer>();
}