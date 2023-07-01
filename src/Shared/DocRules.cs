using System.Collections.Immutable;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using AndNet.Manager.Shared.Models.Documentation.Info.Report;
using static AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition.DecisionCouncilExpeditionPlayer;
using static AndNet.Manager.Shared.Models.Documentation.Info.DirectiveInfo;

namespace AndNet.Manager.Shared;

public static class DocRules
{
    public static readonly ImmutableDictionary<Type, string> DocNames = ImmutableDictionary<Type, string>.Empty
        .Add(typeof(DocInfo), "Документ")
        .Add(typeof(DirectiveInfo), "Директива")
        .Add(typeof(ProtocolInfo), "Протокол")
        .Add(typeof(ReportInfo), "Отчёт")
        .Add(typeof(ReportInfoExpedition), "Отчёт экспедиции")
        .Add(typeof(ReportInfoBattle), "Отчёт о битве")
        .Add(typeof(Decision), "Решение")
        .Add(typeof(DecisionGeneralMeeting), "Решение общего сбора")
        .Add(typeof(DecisionCouncil), "Решение совета")
        .Add(typeof(DecisionCouncilGeneralMeetingInit), "Решение совета о созыве общего сбора")
        .Add(typeof(DecisionCouncilDirective), "Решение совета о директиве")
        .Add(typeof(DecisionCouncilDirectiveChange), "Решение совета о замене директивы")
        .Add(typeof(DecisionCouncilExpedition), "Решение совета о экспедиции")
        .Add(typeof(DecisionCouncilExpeditionClose), "Решение совета о роспуске экспедиции")
        .Add(typeof(DecisionCouncilExpeditionCreate), "Решение совета о созыве экспедиции")
        .Add(typeof(DecisionCouncilExpeditionPlayer), "Решение совета об участнике экспедиции")
        .Add(typeof(DecisionCouncilExpeditionProlongation), "Решение совета о продлении экспедиции")
        .Add(typeof(DecisionCouncilPlayer), "Решение совета о игроке")
        .Add(typeof(DecisionCouncilPlayerAcceptApplication), "Решение совета о включении игрока в состав клана")
        .Add(typeof(DecisionCouncilPlayerAwardSheet), "Решение совета о присвоении награды (штрафа) игроку")
        .Add(typeof(DecisionCouncilPlayerChange), "Решение об изменении данных игрока")
        .Add(typeof(DecisionCouncilPlayerKick), "Решение совета о прекращении членства игрока в клане");

    public static readonly ImmutableDictionary<VoteType, string> VoteNames = ImmutableDictionary<VoteType, string>.Empty
        .Add(VoteType.None, "Нет голоса")
        .Add(VoteType.No, "Против")
        .Add(VoteType.Yes, "За")
        .Add(VoteType.Abstain, "Воздежался(ась)")
        .Add(VoteType.NeedMoreInfo, "Нужно больше информации");

    public static readonly ImmutableDictionary<DirectiveStatus, string> DirectiveStatusNames =
        ImmutableDictionary<DirectiveStatus, string>.Empty
            .Add(DirectiveStatus.Canceled, "Отменена")
            .Add(DirectiveStatus.Replaced, "Заменена")
            .Add(DirectiveStatus.Project, "Не принята")
            .Add(DirectiveStatus.Accepted, "Принята");

    public static readonly ImmutableDictionary<DecisionCouncilDirective.DirectiveAction, string> DirectiveActionNames =
        ImmutableDictionary<DecisionCouncilDirective.DirectiveAction, string>.Empty
            .Add(DecisionCouncilDirective.DirectiveAction.Accept, "Принять")
            .Add(DecisionCouncilDirective.DirectiveAction.Cancel, "Отменить")
            .Add(DecisionCouncilDirective.DirectiveAction.Generic, "Неизвестно");

    public static readonly ImmutableDictionary<ProtocolType, string> ProtocolTypeNames =
        ImmutableDictionary<ProtocolType, string>.Empty
            .Add(ProtocolType.Unknown, "Неизвестно")
            .Add(ProtocolType.Council, "Совет")
            .Add(ProtocolType.GeneralMeeting, "Общий сбор");

    public static readonly ImmutableDictionary<ExpeditionPlayerAction, string> ExpeditionPlayerActionNames =
        ImmutableDictionary<ExpeditionPlayerAction, string>.Empty
            .Add(ExpeditionPlayerAction.Unknown, "Неизвестно")
            .Add(ExpeditionPlayerAction.Add, "Включить игрока в состав")
            .Add(ExpeditionPlayerAction.Remove, "Исключить игрока из состава")
            .Add(ExpeditionPlayerAction.ChangeCommander, "Назначить нового командира");
}