namespace AndNet.Manager.Shared.Enums;

public enum ElectionStage : byte
{
    NotStarted,
    Registration = 16,
    Voting = 32,
    ResultsAnnounce = 48,
    Ended = byte.MaxValue
}