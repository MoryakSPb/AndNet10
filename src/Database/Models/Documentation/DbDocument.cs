using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Player;
using NpgsqlTypes;

namespace AndNet.Manager.Database.Models.Documentation;

public record DbDocument
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public int CreatorId { get; set; }
    public DbPlayer Creator { get; set; } = null!;
    public virtual string Prefix { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public DbDocument? Parent { get; set; }
    public IList<DbDocument> ChildrenDocuments { get; set; } = Array.Empty<DbDocument>();

    [JsonIgnore]
    public NpgsqlTsVector SearchVector { get; set; }

    public uint Version { get; set; }
}