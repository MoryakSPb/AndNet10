using System.Collections.Immutable;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared;

public static class PlayerRules
{
    public static readonly IReadOnlyDictionary<PlayerRelationship, string> RelationshipNames =
        ImmutableSortedDictionary<PlayerRelationship, string>
            .Empty
            .Add(PlayerRelationship.Ally, "Союзник")
            .Add(PlayerRelationship.Neutral, "Нейтрал")
            .Add(PlayerRelationship.Enemy, "Враг")
            .Add(PlayerRelationship.Unknown, "(Неизвестно)");

    public static readonly IReadOnlyDictionary<PlayerLeaveReason, string> LeaveReasonNames =
        ImmutableSortedDictionary<PlayerLeaveReason, string>
            .Empty
            .Add(PlayerLeaveReason.AtWill, "По желанию")
            .Add(PlayerLeaveReason.Exclude, "Исключение")
            .Add(PlayerLeaveReason.Suspend, "Приостановка")
            .Add(PlayerLeaveReason.Exile, "Изгнание")
            .Add(PlayerLeaveReason.Unknown, "Нет данных");

    public static readonly IReadOnlyDictionary<PlayerStatisticsStatus, string> PlayerStatisticsStatusNames =
        ImmutableSortedDictionary<PlayerStatisticsStatus, string>
            .Empty
            .Add(PlayerStatisticsStatus.Unknown, string.Empty)
            .Add(PlayerStatisticsStatus.Offline, "Не в сети")
            .Add(PlayerStatisticsStatus.Online, "В сети")
            .Add(PlayerStatisticsStatus.InDifferentGame, "В другой игре")
            .Add(PlayerStatisticsStatus.InSpaceEngineers, "В игре")
            .Add(PlayerStatisticsStatus.InSpaceEngineersWithComrade, "В игре с кланом");
}