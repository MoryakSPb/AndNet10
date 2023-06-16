using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Documentation.Decisions.Utility;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Database.Models.Documentation.Decisions;

public record DbDocumentDecision : DbDocument
{
    public override string Prefix { get; set; } = "Р";
    public IList<DbVote> Votes { get; set; } = new List<DbVote>();
    public double MinYesVotesPercent { get; set; } = 1d;

    [JsonIgnore]
    public virtual int ActualVotes => Votes.Count(x => x.VoteType is VoteType.Yes or VoteType.No);

    [JsonIgnore]
    public int BlockingVotes => Votes.Count(x => x.VoteType is VoteType.NeedMoreInfo);

    [JsonIgnore]
    public int YesVotes => Votes.Count(x => x.VoteType is VoteType.Yes);

    [JsonIgnore]
    public int NoVotes => Votes.Count(x => x.VoteType is VoteType.No);

    [JsonIgnore]
    public double AgreePercent => (double)YesVotes / ActualVotes;

    [JsonIgnore]
    public bool IsAgreeAvailable => AgreePercent >= MinYesVotesPercent && BlockingVotes == 0 && !IsExecuted.HasValue;

    [JsonIgnore]
    public bool IsDeclineAvailable =>
        !(AgreePercent >= MinYesVotesPercent) && BlockingVotes == 0 && !IsExecuted.HasValue;

    public bool? IsExecuted { get; set; }
    public int? ExecutorId { get; set; }
    public DbPlayer? Executor { get; set; }
    public DateTime? ExecuteDate { get; set; }

    public async Task AgreeExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        if (signer.Rank < PlayerRank.Advisor) throw new ArgumentOutOfRangeException(nameof(signer));
        if (!IsAgreeAvailable) throw new InvalidOperationException();
        ExecuteDate = DateTime.UtcNow;
        Executor = signer;
        ExecutorId = signer.Id;
        await ExecuteAsync(signer, context).ConfigureAwait(false);
        IsExecuted = true;
    }

    public async Task DeclineExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        if (signer.Rank < PlayerRank.Advisor) throw new ArgumentOutOfRangeException(nameof(signer));
        if (IsExecuted.HasValue) throw new InvalidOperationException();
        if (!IsDeclineAvailable) throw new InvalidOperationException();
        ExecuteDate = DateTime.UtcNow;
        Executor = signer;
        ExecutorId = signer.Id;
        await ExecuteDeclineAsync(signer, context).ConfigureAwait(false);
        IsExecuted = false;
    }

    protected virtual Task ExecuteAsync(DbClanPlayer signer, DatabaseContext context)
    {
        return Task.CompletedTask;
    }

    protected virtual Task ExecuteDeclineAsync(DbClanPlayer signer, DatabaseContext context)
    {
        return Task.CompletedTask;
    }
}