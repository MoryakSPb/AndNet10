using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision;

[JsonDerivedType(typeof(DecisionCouncilGeneralMeetingInit), "РCО")]
public record DecisionCouncilGeneralMeetingInit : DecisionCouncil
{
    public DateTime StartDate { get; set; }
}