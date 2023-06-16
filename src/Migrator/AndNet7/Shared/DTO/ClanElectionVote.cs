namespace AndNet.Migrator.AndNet7.AndNet7.Shared.DTO;

public class ClanElectionVote
{
    public ClanElectionCode Code { get; set; } = null!;
    public List<ClanElectionVoteCandidate> Votes { get; set; } = null!;
    public int AgainstAll { get; set; }
}