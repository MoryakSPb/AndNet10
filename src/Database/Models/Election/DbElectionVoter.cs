using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Election;

public record DbElectionVoter
{
    public int ElectionId { get; set; }

    [JsonIgnore]
    public DbElection Election { get; set; } = null!;

    public int PlayerId { get; set; }

    [JsonIgnore]
    public DbPlayer Player { get; set; } = null!;

    public DateTime? VoteDate { get; set; }
    public uint Version { get; set; }
}