using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models;

public record DbPlayerContact
{
    [JsonIgnore]
    public DbPlayer Player { get; set; } = null!;

    public int PlayerId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public uint Version { get; set; }

    [JsonIgnore]

    public bool IsMarkedForDelete { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(PlayerId, Type);
    }
}