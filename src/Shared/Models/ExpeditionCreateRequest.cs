using System.Collections.Immutable;

namespace AndNet.Manager.Shared.Models;

public sealed record ExpeditionCreateRequest
{
    public int Days { get; set; }
    public ImmutableArray<int> Members { get; set; }
    public string Description { get; set; } = string.Empty;
}