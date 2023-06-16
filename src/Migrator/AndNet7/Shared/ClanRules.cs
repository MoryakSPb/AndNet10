using System.Collections.ObjectModel;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared;

public static class ClanRules
{
    public const int ADVISOR_TERM_DAYS = 90;
    public const int DRUZHINA_PLAYERS_LIMIT = 8;
    public const int PROGRAM_PLAYERS_LIMIT = 8;

    public static readonly IReadOnlyDictionary<ClanMemberRankEnum, int> RankPoints =
        new ReadOnlyDictionary<ClanMemberRankEnum, int>(new Dictionary<ClanMemberRankEnum, int>(new[]
        {
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.Neophyte, 0),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.Trainee, 1),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.Assistant, 5),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.JuniorEmployee, 15),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.Employee, 25),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.SeniorEmployee, 50),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.Specialist, 75),
            new KeyValuePair<ClanMemberRankEnum, int>(ClanMemberRankEnum.Defender, 100)
        }));

    public static ClanMemberRankEnum GetRank(this IEnumerable<ClanAward> awards)
    {
        int result = awards.Sum(x => (int)x.Type);
        return RankPoints.Where(x => x.Value <= result).OrderByDescending(x => x.Value).First().Key;
    }

    public static string? GetRankIcon(this ClanMemberRankEnum rank)
    {
        return rank switch
        {
            ClanMemberRankEnum.Outcast => null,
            ClanMemberRankEnum.Enemy => null,
            ClanMemberRankEnum.Guest => null,
            ClanMemberRankEnum.Diplomat => null,
            ClanMemberRankEnum.Ally => null,
            ClanMemberRankEnum.Candidate => null,
            ClanMemberRankEnum.None => null,
            ClanMemberRankEnum.Neophyte => "⦁",
            ClanMemberRankEnum.Trainee => "❮❮❮",
            ClanMemberRankEnum.Assistant => "❮❮",
            ClanMemberRankEnum.JuniorEmployee => "❮",
            ClanMemberRankEnum.Employee => "❙❙❙",
            ClanMemberRankEnum.SeniorEmployee => "❙❙",
            ClanMemberRankEnum.Specialist => "❙",
            ClanMemberRankEnum.Defender => "⛉",
            ClanMemberRankEnum.Lieutenant => "◇",
            ClanMemberRankEnum.Captain => "◆",
            ClanMemberRankEnum.Advisor => "△",
            ClanMemberRankEnum.FirstAdvisor => "▲",
            _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
        };
    }

    public static string? GetAsciiRankIcon(this ClanMemberRankEnum rank)
    {
        return rank switch
        {
            ClanMemberRankEnum.Outcast => null,
            ClanMemberRankEnum.Enemy => null,
            ClanMemberRankEnum.Guest => null,
            ClanMemberRankEnum.Diplomat => null,
            ClanMemberRankEnum.Ally => null,
            ClanMemberRankEnum.Candidate => null,
            ClanMemberRankEnum.None => null,
            ClanMemberRankEnum.Neophyte => "O",
            ClanMemberRankEnum.Trainee => "VVV",
            ClanMemberRankEnum.Assistant => "VV",
            ClanMemberRankEnum.JuniorEmployee => "V",
            ClanMemberRankEnum.Employee => "III",
            ClanMemberRankEnum.SeniorEmployee => "II",
            ClanMemberRankEnum.Specialist => "I",
            ClanMemberRankEnum.Defender => "D",
            ClanMemberRankEnum.Lieutenant => "L",
            ClanMemberRankEnum.Captain => "C",
            ClanMemberRankEnum.Advisor => "A",
            ClanMemberRankEnum.FirstAdvisor => "1A",
            _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
        };
    }

    public static string GetName(this ClanDepartmentEnum department)
    {
        return department switch
        {
            ClanDepartmentEnum.Reserve => "Резерв",
            ClanDepartmentEnum.BeginnersPool => "Пул новичков",
            ClanDepartmentEnum.None => "Н/Д",
            ClanDepartmentEnum.Infrastructure => "Отдел развития и инфраструктуры",
            ClanDepartmentEnum.Research => "Отдел исследований и разработок",
            ClanDepartmentEnum.Military => "Военный отдел",
            ClanDepartmentEnum.Agitation => "Отдел агитации и внешних связей",
            _ => throw new ArgumentOutOfRangeException(nameof(department), department, null)
        };
    }

    public static string GetRankName(this ClanMemberRankEnum rank)
    {
        return rank switch
        {
            ClanMemberRankEnum.Outcast => "Изганнник",
            ClanMemberRankEnum.Guest => "Гость",
            ClanMemberRankEnum.Diplomat => "Дипломат",
            ClanMemberRankEnum.Ally => "Союзник",
            ClanMemberRankEnum.Candidate => "Кандидат",
            ClanMemberRankEnum.None => "",
            ClanMemberRankEnum.Neophyte => "Неофит",
            ClanMemberRankEnum.Trainee => "Стажёр",
            ClanMemberRankEnum.Assistant => "Ассистент",
            ClanMemberRankEnum.JuniorEmployee => "Младший сотрудник",
            ClanMemberRankEnum.Employee => "Сотрудник",
            ClanMemberRankEnum.SeniorEmployee => "Старший сорудник",
            ClanMemberRankEnum.Specialist => "Специалист",
            ClanMemberRankEnum.Defender => "Защитник",
            ClanMemberRankEnum.Lieutenant => "Лейтенант",
            ClanMemberRankEnum.Captain => "Капитан",
            ClanMemberRankEnum.Advisor => "Советник",
            ClanMemberRankEnum.FirstAdvisor => "Первый советник",
            _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
        };
    }
}