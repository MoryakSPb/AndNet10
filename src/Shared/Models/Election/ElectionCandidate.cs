namespace AndNet.Manager.Shared.Models.Election;

public record ElectionCandidate
{
    public int PlayerId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int Rating { get; set; }
    public bool IsWinner { get; set; }
}