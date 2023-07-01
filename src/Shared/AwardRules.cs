using System.Collections.Immutable;
using System.Runtime.InteropServices;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared;

public class AwardRules
{
    public static readonly IReadOnlyDictionary<AwardType, string> Icons = ImmutableSortedDictionary<AwardType, string>
        .Empty
        .Add(AwardType.Hero, "🟪")
        .Add(AwardType.Sapphire, "🟦")
        .Add(AwardType.Gold, "🟨")
        .Add(AwardType.Silver, "⬜")
        .Add(AwardType.Bronze, "🟧")
        .Add(AwardType.Copper, "🟩")
        .Add(AwardType.SmallPenalty, "🞬")
        .Add(AwardType.MediumPenalty, "🞸")
        .Add(AwardType.LargePenalty, "🞾");

    //↓
    //⇓
    //⩣⤋
    public static readonly IReadOnlyDictionary<AwardType, string> Names = ImmutableSortedDictionary<AwardType, string>
        .Empty
        .Add(AwardType.Hero, "Герой")
        .Add(AwardType.Sapphire, "Сапфир")
        .Add(AwardType.Gold, "Золото")
        .Add(AwardType.Silver, "Серебро")
        .Add(AwardType.Bronze, "Бронза")
        .Add(AwardType.Copper, "Медь")
        .Add(AwardType.SmallPenalty, "Малый штраф")
        .Add(AwardType.MediumPenalty, "Средний штраф")
        .Add(AwardType.LargePenalty, "Большой штраф");

    public static readonly IReadOnlyDictionary<AwardType, double> MinCouncilVotes =
        ImmutableSortedDictionary<AwardType, double>
            .Empty
            .Add(AwardType.Hero, double.NaN)
            .Add(AwardType.Sapphire, 1.00)
            .Add(AwardType.Gold, 0.75)
            .Add(AwardType.Silver, 0.50)
            .Add(AwardType.Bronze, RuntimeInformation.ProcessArchitecture.ToString("G")
                .Contains("ARM", StringComparison.OrdinalIgnoreCase)
                ? 2.2250738585072014E-308
                : double.Epsilon)
            .Add(AwardType.Copper, 0.00)
            .Add(AwardType.SmallPenalty, 0.50)
            .Add(AwardType.MediumPenalty, 0.75)
            .Add(AwardType.LargePenalty, 1.00);
}