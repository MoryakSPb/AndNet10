using System.Collections.Immutable;

namespace AndNet.Manager.Shared.Models.Documentation;

public record Doc
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public int AuthorId { get; set; }
    public int? ParentId { get; set; }
    public int Views { get; set; }
    public DocInfo? Info { get; set; }
    public virtual IReadOnlyCollection<int> ChildIds { get; set; } = ImmutableArray<int>.Empty;
    public uint Version { get; set; }
}