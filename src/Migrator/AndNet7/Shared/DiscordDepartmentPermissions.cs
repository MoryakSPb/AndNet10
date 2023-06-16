using System.Text.Json.Serialization;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared;

public class DiscordDepartmentPermissions
{
    [JsonIgnore]
    public ulong ChannelId { get; set; }

    public ClanDepartmentEnum Department { get; set; }

    [JsonIgnore]
    public virtual DiscordChannelMetadata Metadata { get; set; }

    public DiscordPermissionsFlags Permissions { get; set; }
}