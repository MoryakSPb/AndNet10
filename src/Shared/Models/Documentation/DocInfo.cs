using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using AndNet.Manager.Shared.Models.Documentation.Info.Report;

namespace AndNet.Manager.Shared.Models.Documentation;

[JsonPolymorphic]
[JsonDerivedType(typeof(DocInfo), "")]
[JsonDerivedType(typeof(DirectiveInfo), "Д")]
[JsonDerivedType(typeof(ProtocolInfo), "П")]
[JsonDerivedType(typeof(ReportInfo), "О")]
[JsonDerivedType(typeof(ReportInfoExpedition), "ОЭ")]
[JsonDerivedType(typeof(ReportInfoBattle), "ОБ")]
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
public record DocInfo
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("$type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type
    {
        get;
        [Obsolete("Значение устанавливает JsonSerializer")]
        set;
    } = null!;
}