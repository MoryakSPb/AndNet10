namespace AndNet.Manager.Shared.Enums;

public enum PlayerRank : sbyte
{
    None = 0,

    Penal = 1 << 4,

    Neophyte = (1 << 5) + 0,
    Trainee = (1 << 5) + 2 * 1,
    Assistant = (1 << 5) + 2 * 2,
    JuniorEmployee = (1 << 5) + 2 * 3,
    MiddleEmployee = (1 << 5) + 2 * 4,
    SeniorEmployee = (1 << 5) + 2 * 5,
    Specialist3rd = (1 << 5) + 2 * 6,
    Specialist2nd = (1 << 5) + 2 * 7,
    Specialist1st = (1 << 5) + 2 * 8,
    Intercessor3rd = (1 << 5) + 2 * 9,
    Intercessor2nd = (1 << 5) + 2 * 10,
    Intercessor1st = (1 << 5) + 2 * 11,
    Guardian3rd = (1 << 5) + 2 * 12,
    Guardian2nd = (1 << 5) + 2 * 13,
    Guardian1st = (1 << 5) + 2 * 14,

    Advisor = 1 << 6,
    FirstAdvisor = sbyte.MaxValue
}