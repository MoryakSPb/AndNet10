using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Election;

public record DbElectionCandidate
{
    public int PlayerId { get; set; }

    [JsonIgnore]
    public int ElectionId { get; set; }

    public DateTime RegistrationDate { get; set; }
    public int Rating { get; set; }
    public bool IsWinner { get; set; }

    [JsonIgnore]
    public DbElection Election { get; set; } = null!;

    [JsonIgnore]
    public DbPlayer Player { get; set; } = null!;

    public uint Version { get; set; }
}