using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Models.Election;

namespace AndNet.Manager.Database.Models.Election;

public record DbElection : Shared.Models.Election.Election
{
    [JsonIgnore]
    public IList<DbElectionCandidate> ElectionCandidates { get; set; } = new List<DbElectionCandidate>();

    public override IImmutableList<ElectionCandidate> Candidates =>
        ElectionCandidates.Select(x => new ElectionCandidate
        {
            IsWinner = x.IsWinner,
            PlayerId = x.PlayerId,
            Rating = x.Rating,
            RegistrationDate = x.RegistrationDate
        }).ToImmutableList();

    [JsonIgnore]
    public IList<DbElectionVoter> Voters { get; set; } = new List<DbElectionVoter>();

    public override int AllVotersCount => Voters.Count;
    public override int VotedVotersCount => Voters.Count(x => x.VoteDate.HasValue);
    public uint Version { get; set; }
}