using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision;

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
public record DecisionCouncil : Decision
{
    [Obsolete]
    public DecisionCouncil GenerateVotes(int[] playerIds)
    {
        Votes = Votes.ExceptBy(Votes
                .Select(x => x.PlayerId).Except(playerIds), x => x.PlayerId)
            .Concat(playerIds
                .Except(Votes.Select(x => x.PlayerId))
                .Select(x => new Vote
                {
                    PlayerId = x,
                    VoteType = VoteType.None,
                    Date = DateTime.UtcNow
                }))
            .ToImmutableList();
        return this;
    }
}