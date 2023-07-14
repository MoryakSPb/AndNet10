using System.Collections.Immutable;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared.Models.Election;

public record Election
{
    public int Id { get; set; }
    public ElectionStage Stage { get; set; }
    public DateTime ElectionEnd { get; set; }
    public int CouncilCapacity { get; set; }
    public virtual int AllVotersCount { get; set; }
    public virtual int VotedVotersCount { get; set; }
    public virtual IReadOnlyCollection<ElectionCandidate> Candidates { get; set; } = ImmutableList<ElectionCandidate>.Empty;
}