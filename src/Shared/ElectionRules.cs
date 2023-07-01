using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared;

public class ElectionRules
{
    public static readonly IReadOnlyDictionary<ElectionStage, string> StageNames =
        new Dictionary<ElectionStage, string>
        {
            { ElectionStage.NotStarted, "Не начаты" },
            { ElectionStage.Registration, "Регистрация" },
            { ElectionStage.Voting, "Голосование" },
            { ElectionStage.ResultsAnnounce, "Объявление итогов" },
            { ElectionStage.Ended, "Закончены" }
        };
}