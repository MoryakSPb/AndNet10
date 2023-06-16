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
    JuniorSpecialist = (1 << 5) + 2 * 6,
    MiddleSpecialist = (1 << 5) + 2 * 7,
    SeniorSpecialist = (1 << 5) + 2 * 8,
    JuniorIntercessor = (1 << 5) + 2 * 9,
    MiddleIntercessor = (1 << 5) + 2 * 10,
    SeniorIntercessor = (1 << 5) + 2 * 11,
    JuniorGuardian = (1 << 5) + 2 * 12,
    MiddleGuardian = (1 << 5) + 2 * 13,
    SeniorGuardian = (1 << 5) + 2 * 14,

    Advisor = 1 << 6,
    FirstAdvisor = sbyte.MaxValue
}