using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;

namespace AndNet.Manager.Client;

public static class DocInfoFields
{
    public enum FieldType
    {
        Text,
        PlayerLink,
        PlayersList,
        ExpeditionLink,
        DocLink
    }

    public static IEnumerable<(string name, string value, FieldType type)> GetFields(this Doc doc)
    {
        yield return ("Автор", doc.AuthorId.ToString("D"), FieldType.PlayerLink);
        yield return ("Дата и время создания", doc.CreationDate.ToString("g"), FieldType.Text);

        if (doc.Info is IPlayerId playerId)
            yield return ("Игрок", playerId.PlayerId.ToString("D"), FieldType.PlayerLink);
        if (doc.Info is IExpeditionId { ExpeditionId: > 0 } expeditionId)
            yield return ("Экспедиция", expeditionId.ExpeditionId.ToString("D"), FieldType.ExpeditionLink);

        switch (doc.Info)
        {
            case ProtocolInfo protocolInfo:
                yield return ("Тип протокола", DocRules.ProtocolTypeNames[protocolInfo.ProtocolType], FieldType.Text);
                break;
            case DirectiveInfo directiveInfo:
                yield return ("Статус директивы", DocRules.DirectiveStatusNames[directiveInfo.Status], FieldType.Text);
                if (directiveInfo.AcceptanceDate.HasValue)
                    yield return ("Дата принятия", directiveInfo.AcceptanceDate.Value.ToShortDateString(),
                        FieldType.Text);
                if (directiveInfo.CancelDate.HasValue)
                    yield return ("Дата отмены", directiveInfo.CancelDate.Value.ToShortDateString(), FieldType.Text);
                if (directiveInfo.ReplacedById.HasValue)
                    yield return ("Заменен на", directiveInfo.ReplacedById.Value.ToString("D"), FieldType.DocLink);
                break;
            case Decision decision:
                yield return ("Статус решения", DocRules.GetDecisionStatus(decision.IsExecuted), FieldType.Text);
                switch (decision)
                {
                    case DecisionCouncilGeneralMeetingInit decisionCouncilGeneralMeetingInit:
                        yield return ("Дата сбора", decisionCouncilGeneralMeetingInit.StartDate.ToString("g"),
                            FieldType.DocLink);
                        break;
                    case DecisionCouncilDirective decisionCouncilDirective:
                        yield return ("Директива", decisionCouncilDirective.DirectiveId.ToString("D"),
                            FieldType.DocLink);
                        if (decisionCouncilDirective is DecisionCouncilDirectiveChange decisionCouncilDirectiveChange)
                            yield return ("Замена на директиву",
                                decisionCouncilDirectiveChange.NewDirectiveId.ToString("D"), FieldType.DocLink);
                        else
                            yield return ("Статус решения",
                                DocRules.DirectiveActionNames[decisionCouncilDirective.Action],
                                FieldType.Text);
                        break;
                    case DecisionCouncilExpeditionCreate decisionCouncilExpeditionCreate:
                        yield return ("Действие над экспедицией", "Создать", FieldType.Text);
                        yield return ("Командир", decisionCouncilExpeditionCreate.AccountablePlayerId.ToString("D"),
                            FieldType.PlayerLink);
                        yield return ("Длительность (сутки)",
                            decisionCouncilExpeditionCreate.Duration.TotalDays.ToString("F0"), FieldType.Text);
                        yield return ("Участники",
                            string.Join(',', decisionCouncilExpeditionCreate.Members.Select(x => x.ToString("D"))),
                            FieldType.PlayersList);
                        break;
                    case DecisionCouncilExpedition decisionCouncilExpedition:
                        switch (decisionCouncilExpedition)
                        {
                            case DecisionCouncilExpeditionClose decisionCouncilExpeditionClose:
                                yield return ("Действие над экспедицией", "Досрочно закрыть", FieldType.Text);
                                break;
                            case DecisionCouncilExpeditionPlayer decisionCouncilExpeditionPlayer:
                                yield return ("Действие над экспедицией",
                                    DocRules.ExpeditionPlayerActionNames[decisionCouncilExpeditionPlayer.Action],
                                    FieldType.Text);
                                break;
                            case DecisionCouncilExpeditionProlongation decisionCouncilExpeditionProlongation:
                                yield return ("Действие над экспедицией", "Продлить", FieldType.Text);
                                yield return ("Дополнительное время (сутки)",
                                    decisionCouncilExpeditionProlongation.ProlongationTime.TotalDays.ToString("F0"),
                                    FieldType.Text);
                                break;
                        }

                        break;
                    case DecisionCouncilPlayer decisionCouncilPlayer:
                        switch (decisionCouncilPlayer)
                        {
                            case DecisionCouncilPlayerAcceptApplication decisionCouncilPlayerAcceptApplication:
                                yield return ("Рекомендация",
                                    string.IsNullOrWhiteSpace(decisionCouncilPlayerAcceptApplication.Recommendation)
                                        ? "Н/Д"
                                        : decisionCouncilPlayerAcceptApplication.Recommendation, FieldType.Text);
                                yield return ("Часов в игре",
                                    decisionCouncilPlayerAcceptApplication.Hours?.ToString("D") ?? "Н/Д",
                                    FieldType.Text);
                                yield return ("Возраст",
                                    decisionCouncilPlayerAcceptApplication.Age?.ToString("D") ?? "Н/Д", FieldType.Text);
                                break;
                            case DecisionCouncilPlayerAwardSheet decisionCouncilPlayerAwardSheet:
                                yield return ("Тип награды",
                                    AwardRules.Names[decisionCouncilPlayerAwardSheet.AwardType], FieldType.Text);
                                yield return ("Описание награды", decisionCouncilPlayerAwardSheet.Description,
                                    FieldType.Text);
                                if (decisionCouncilPlayerAwardSheet.AutomationId is not null)
                                    yield return ("Идентификатор автоматизации",
                                        decisionCouncilPlayerAwardSheet.AutomationId.Value.ToString("D"),
                                        FieldType.Text);
                                break;
                            case DecisionCouncilPlayerChange decisionCouncilPlayerChange:
                                break;
                            case DecisionCouncilPlayerKick decisionCouncilPlayerKick:
                                yield return ("Тип прекращения членства",
                                    PlayerRules.LeaveReasonNames[decisionCouncilPlayerKick.PlayerLeaveReason],
                                    FieldType.Text);
                                break;
                        }

                        break;
                }

                break;
        }

        if (doc.Info is Decision { Votes.Count: > 0, MinYesVotesPercent: > 0.0 } voteDecision)
        {
            yield return ("Порог принятия",
                voteDecision.MinYesVotesPercent < 0.01 ? "1 голос" : voteDecision.MinYesVotesPercent.ToString("P0"),
                FieldType.Text);
            yield return ("Решение",
                voteDecision.IsExecuted.HasValue
                    ? voteDecision.IsExecuted.Value ? "Исполнено" : "Отклонено"
                    : "На рассмотрении", FieldType.Text);
            if (voteDecision.IsExecuted == true)
            {
                if (voteDecision.ExecutorId is not null)
                    yield return ("Исполнитель",
                        voteDecision.ExecutorId.Value.ToString("D"),
                        FieldType.PlayerLink);
                if (voteDecision.ExecuteDate is not null)
                    yield return ("Дата исполнения",
                        voteDecision.ExecuteDate.Value.ToString("g"),
                        FieldType.Text);
            }
        }
    }
}