using System.Collections.Immutable;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared;

public static class RankRules
{
    public static readonly IReadOnlyDictionary<PlayerRank, string> Icons = ImmutableSortedDictionary<PlayerRank, string>
        .Empty
        .Add(PlayerRank.FirstAdvisor, "▲")
        .Add(PlayerRank.Advisor, "△")
        .Add(PlayerRank.SeniorGuardian, "★")
        .Add(PlayerRank.MiddleGuardian, "★★")
        .Add(PlayerRank.JuniorGuardian, "★★★")
        .Add(PlayerRank.SeniorIntercessor, "☆")
        .Add(PlayerRank.MiddleIntercessor, "☆☆")
        .Add(PlayerRank.JuniorIntercessor, "☆☆☆")
        .Add(PlayerRank.SeniorSpecialist, "❙")
        .Add(PlayerRank.MiddleSpecialist, "❙❙")
        .Add(PlayerRank.JuniorSpecialist, "❙❙❙")
        .Add(PlayerRank.SeniorEmployee, "❮")
        .Add(PlayerRank.MiddleEmployee, "❮❮")
        .Add(PlayerRank.JuniorEmployee, "❮❮❮")
        .Add(PlayerRank.Assistant, "⦁")
        .Add(PlayerRank.Trainee, "⦁⦁")
        .Add(PlayerRank.Neophyte, "⦁⦁⦁")
        .Add(PlayerRank.Penal, "⦿");

    public static readonly IReadOnlyDictionary<PlayerRank, string> Names = ImmutableSortedDictionary<PlayerRank, string>
        .Empty
        .Add(PlayerRank.FirstAdvisor, "Первый советник")
        .Add(PlayerRank.Advisor, "Советник")
        .Add(PlayerRank.SeniorGuardian, "Старший страж")
        .Add(PlayerRank.MiddleGuardian, "Страж")
        .Add(PlayerRank.JuniorGuardian, "Младший страж")
        .Add(PlayerRank.SeniorIntercessor, "Старший заступник")
        .Add(PlayerRank.MiddleIntercessor, "Заступник")
        .Add(PlayerRank.JuniorIntercessor, "Младший заступник")
        .Add(PlayerRank.SeniorSpecialist, "Старший специалист")
        .Add(PlayerRank.MiddleSpecialist, "Специалист")
        .Add(PlayerRank.JuniorSpecialist, "Младший специалист")
        .Add(PlayerRank.SeniorEmployee, "Старший сотрудник")
        .Add(PlayerRank.MiddleEmployee, "Сотрудник")
        .Add(PlayerRank.JuniorEmployee, "Младший сотрудник")
        .Add(PlayerRank.Assistant, "Ассистент")
        .Add(PlayerRank.Trainee, "Стажёр")
        .Add(PlayerRank.Neophyte, "Неофит")
        .Add(PlayerRank.Penal, "Штрафник");

    public static readonly IReadOnlyDictionary<double, PlayerRank> MinimalScores =
        ImmutableSortedDictionary<double, PlayerRank>
            .Empty
            .Add(150d, PlayerRank.SeniorGuardian)
            .Add(135d, PlayerRank.MiddleGuardian)
            .Add(120d, PlayerRank.JuniorGuardian)
            .Add(105d, PlayerRank.SeniorIntercessor)
            .Add(085d, PlayerRank.MiddleIntercessor)
            .Add(070d, PlayerRank.JuniorIntercessor)
            .Add(055d, PlayerRank.SeniorSpecialist)
            .Add(045d, PlayerRank.MiddleSpecialist)
            .Add(035d, PlayerRank.JuniorSpecialist)
            .Add(025d, PlayerRank.SeniorEmployee)
            .Add(020d, PlayerRank.MiddleEmployee)
            .Add(015d, PlayerRank.JuniorEmployee)
            .Add(010d, PlayerRank.Assistant)
            .Add(005d, PlayerRank.Trainee)
            .Add(000d, PlayerRank.Neophyte)
            .Add(double.NegativeInfinity, PlayerRank.Penal);
}