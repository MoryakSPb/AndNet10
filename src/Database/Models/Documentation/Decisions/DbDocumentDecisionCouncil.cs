using AndNet.Manager.Database.Models.Documentation.Decisions.Utility;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Database.Models.Documentation.Decisions;

public record DbDocumentDecisionCouncil : DbDocumentDecision
{
    public override string Prefix { get; set; } = "РС";
    public override int ActualVotes => Votes.Count(x => x.VoteType is VoteType.Yes or VoteType.No);

    public void GenerateVotes(DatabaseContext databaseContext)
    {
        List<DbVote> oldVotes = Votes.ToList();
        List<DbVote> newVotes = new(8);
        foreach (DbClanPlayer clanPlayer in databaseContext.ClanPlayers.Where(x => x.Rank >= PlayerRank.Advisor))
        {
            DbVote? vote = oldVotes.FirstOrDefault(x => x.PlayerId == clanPlayer.Id);
            vote ??= new()
            {
                DecisionId = Id,
                PlayerId = clanPlayer.Id,
                VoteType = VoteType.None,
                Date = DateTime.UtcNow
            };
            newVotes.Add(vote);
        }

        Votes.Clear();
        foreach (DbVote newVote in newVotes) Votes.Add(newVote);
    }
}