namespace AndNet.Integration.Discord.Options;

public class DiscordOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public ulong GuildId { get; set; }

    public ulong ClanRoleId { get; set; }
    public ulong AdvisorRoleId { get; set; }
    public ulong ReserveRoleId { get; set; }

    public ulong LogBotChannelId { get; set; }
    public ulong ExpeditionsCategoryId { get; set; }
}