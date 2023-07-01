using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Decision;

[JsonDerivedType(typeof(DecisionGeneralMeeting), "РО")]
public record DecisionGeneralMeeting : Decision
{
}