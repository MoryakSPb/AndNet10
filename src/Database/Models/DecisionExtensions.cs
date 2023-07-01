using System.Collections.Immutable;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;

namespace AndNet.Manager.Database.Models;

public static class DecisionExtensions
{
    public static void AgreeExecuteAsync(this Decision info, DbClanPlayer signer)
    {
        if (signer.Rank < PlayerRank.Advisor) throw new ArgumentOutOfRangeException(nameof(signer));
        if (!info.IsAgreeAvailable) throw new InvalidOperationException();
        info.ExecuteDate = DateTime.UtcNow;
        info.ExecutorId = signer.Id;
        info.IsExecuted = true;
    }

    public static void DeclineExecuteAsync(this Decision info, DbClanPlayer signer)
    {
        if (signer.Rank < PlayerRank.Advisor) throw new ArgumentOutOfRangeException(nameof(signer));
        if (info.IsExecuted.HasValue) throw new InvalidOperationException();
        if (!info.IsDeclineAvailable) throw new InvalidOperationException();
        info.ExecuteDate = DateTime.UtcNow;
        info.ExecutorId = signer.Id;
        info.IsExecuted = false;
    }

    public static DecisionCouncil GenerateVotes(this DecisionCouncil info, DatabaseContext databaseContext)
    {
        ImmutableList<Decision.Vote> oldVotes = info.Votes;
        ImmutableList<Decision.Vote>.Builder newVotes = ImmutableList<Decision.Vote>.Empty.ToBuilder();
        foreach (DbClanPlayer clanPlayer in databaseContext.ClanPlayers.Where(x => x.Rank >= PlayerRank.Advisor))
        {
            Decision.Vote? vote = oldVotes.FirstOrDefault(x => x.PlayerId == clanPlayer.Id);
            vote ??= new()
            {
                PlayerId = clanPlayer.Id,
                VoteType = VoteType.None,
                Date = DateTime.UtcNow
            };
            newVotes.Add(vote);
        }

        info.Votes = newVotes.ToImmutable();
        return info;
    }
}