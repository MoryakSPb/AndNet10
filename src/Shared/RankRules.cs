using System.Collections.Immutable;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared;

public static class RankRules
{
    public static readonly IReadOnlyDictionary<PlayerRank, string> Icons = ImmutableSortedDictionary<PlayerRank, string>
        .Empty
        .Add(PlayerRank.FirstAdvisor, "▲")
        .Add(PlayerRank.Advisor, "△")
        .Add(PlayerRank.Guardian1st, "★")
        .Add(PlayerRank.Guardian2nd, "★★")
        .Add(PlayerRank.Guardian3rd, "★★★")
        .Add(PlayerRank.Intercessor1st, "☆")
        .Add(PlayerRank.Intercessor2nd, "☆☆")
        .Add(PlayerRank.Intercessor3rd, "☆☆☆")
        .Add(PlayerRank.Specialist1st, "❙")
        .Add(PlayerRank.Specialist2nd, "❙❙")
        .Add(PlayerRank.Specialist3rd, "❙❙❙")
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
        .Add(PlayerRank.Guardian1st, "Страж 1-го ранга")
        .Add(PlayerRank.Guardian2nd, "Страж 2-го ранга")
        .Add(PlayerRank.Guardian3rd, "Страж 3-го ранга")
        .Add(PlayerRank.Intercessor1st, "Заступник 1-го ранга")
        .Add(PlayerRank.Intercessor2nd, "Заступник 2-го ранга")
        .Add(PlayerRank.Intercessor3rd, "Заступник 3-го ранга")
        .Add(PlayerRank.Specialist1st, "Специалист 1-ой статьи")
        .Add(PlayerRank.Specialist2nd, "Специалист 2-ой статьи")
        .Add(PlayerRank.Specialist3rd, "Специалист 3-ей статьи")
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
            .Add(150d, PlayerRank.Guardian1st)
            .Add(135d, PlayerRank.Guardian2nd)
            .Add(120d, PlayerRank.Guardian3rd)
            .Add(105d, PlayerRank.Intercessor1st)
            .Add(085d, PlayerRank.Intercessor2nd)
            .Add(070d, PlayerRank.Intercessor3rd)
            .Add(055d, PlayerRank.Specialist1st)
            .Add(045d, PlayerRank.Specialist2nd)
            .Add(035d, PlayerRank.Specialist3rd)
            .Add(025d, PlayerRank.SeniorEmployee)
            .Add(020d, PlayerRank.MiddleEmployee)
            .Add(015d, PlayerRank.JuniorEmployee)
            .Add(010d, PlayerRank.Assistant)
            .Add(005d, PlayerRank.Trainee)
            .Add(000d, PlayerRank.Neophyte)
            .Add(double.NegativeInfinity, PlayerRank.Penal);
}