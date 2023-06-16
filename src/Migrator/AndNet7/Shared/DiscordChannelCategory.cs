using System.Text.Json.Serialization;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared;

public class DiscordChannelCategory
{
    public ulong DiscordId { get; set; }
    public int Position { get; set; }
    public string Name { get; set; }

    [JsonIgnore]
    public virtual IList<DiscordChannelMetadata> Channels { get; set; }
}