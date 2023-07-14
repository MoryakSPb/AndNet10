using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision;

[JsonDerivedType(typeof(Decision), "Р")]
[JsonDerivedType(typeof(DecisionGeneralMeeting), "РО")]
[JsonDerivedType(typeof(DecisionCouncil), "РC")]
[JsonDerivedType(typeof(DecisionCouncilGeneralMeetingInit), "РCО")]
[JsonDerivedType(typeof(DecisionCouncilDirective), "РCД")]
[JsonDerivedType(typeof(DecisionCouncilDirectiveChange), "РCДЗ")]
[JsonDerivedType(typeof(DecisionCouncilExpedition), "РCЭ")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionClose), "РCЭЗ")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionCreate), "РCЭС")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionPlayer), "РCЭИ")]
[JsonDerivedType(typeof(DecisionCouncilExpeditionProlongation), "РCЭП")]
[JsonDerivedType(typeof(DecisionCouncilPlayer), "РCИ")]
[JsonDerivedType(typeof(DecisionCouncilPlayerAcceptApplication), "РCИП")]
[JsonDerivedType(typeof(DecisionCouncilPlayerAwardSheet), "РCИН")]
[JsonDerivedType(typeof(DecisionCouncilPlayerChange), "РCИЗ")]
[JsonDerivedType(typeof(DecisionCouncilPlayerKick), "РCИИ")]
public record Decision : DocInfo
{
    public IReadOnlyCollection<Vote> Votes { get; set; } = ImmutableList<Vote>.Empty;
    public double MinYesVotesPercent { get; set; } = 1d;
    public bool? IsExecuted { get; set; }
    public int? ExecutorId { get; set; }
    public DateTime? ExecuteDate { get; set; }


    [JsonIgnore]
    public virtual int ActualVotes => Votes.Count(x => x.VoteType is not VoteType.Abstain);

    [JsonIgnore]
    public int BlockingVotes => Votes.Count(x => x.VoteType is VoteType.NeedMoreInfo);

    [JsonIgnore]
    public int YesVotes => Votes.Count(x => x.VoteType is VoteType.Yes);

    [JsonIgnore]
    public int NoVotes => Votes.Count(x => x.VoteType is VoteType.No);

    [JsonIgnore]
    public double VotedPercent => Votes.Count(x => x.VoteType is not VoteType.None);

    [JsonIgnore]
    public double AgreePercent => (double)YesVotes / ActualVotes;

    [JsonIgnore]
    public double DeclinePercent => (double)NoVotes / ActualVotes;

    [JsonIgnore]
    public bool IsAgreeAvailable => MinYesVotesPercent == 0d
                                    || (AgreePercent >= MinYesVotesPercent && BlockingVotes == 0
                                                                           && !IsExecuted.HasValue);

    [JsonIgnore]
    public bool IsDeclineAvailable => !IsAgreeAvailable
                                      && (DeclinePercent > MinYesVotesPercent
                                          || Votes.Count(x => x.VoteType is VoteType.None or VoteType.Yes)
                                          / (double)ActualVotes < MinYesVotesPercent)
                                      && BlockingVotes == 0
                                      && !IsExecuted.HasValue;

    public record Vote
    {
        public int PlayerId { get; set; }
        public DateTime Date { get; set; }
        public VoteType VoteType { get; set; } = VoteType.None;
    }
}