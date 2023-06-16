using AndNet.Integration.Discord.Options;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AndNet.Integration.Discord.Services;

public class DiscordService : IHostedService, IDisposable
{
    private readonly ILogger<DiscordService> _logger;
    private readonly IOptions<DiscordOptions> _options;

    public DiscordService(IOptions<DiscordOptions> options, ILogger<DiscordService> logger)
    {
        _options = options;
        _logger = logger;
    }

    internal DiscordSocketClient DiscordClient { get; } = new(new()
    {
        DefaultRetryMode = RetryMode.AlwaysRetry,
        LogLevel = LogSeverity.Debug,
        LogGatewayIntentWarnings = true,
        GatewayIntents = GatewayIntents.All
    });

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DiscordClient.Dispose();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        DiscordClient.Log += DiscordClientOnLog;
        await DiscordClient.LoginAsync(TokenType.Bot, _options.Value.ApiKey).ConfigureAwait(true);
        await DiscordClient.StartAsync().ConfigureAwait(true);
        //_logger.LogInformation("Loggined as «{username}»", DiscordClient.CurrentUser.Username);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DiscordClient.StopAsync().ConfigureAwait(true);
        await DiscordClient.LogoutAsync().ConfigureAwait(true);
    }

    private Task DiscordClientOnLog(LogMessage arg)
    {
        LogLevel logLevel = arg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Critical,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => throw new ArgumentOutOfRangeException(nameof(arg))
        };
        _logger.Log(logLevel, arg.Exception, arg.Message);
        return Task.CompletedTask;
    }

    public async Task SendDirectMessageAsync(ulong discordId, string message,
        CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;
        RestUser user = await DiscordClient.Rest.GetUserAsync(discordId, requestOptions).ConfigureAwait(false)
                        ?? throw new ArgumentOutOfRangeException(nameof(discordId), "User not found");
        try
        {
            await user.SendMessageAsync(message, options: requestOptions).ConfigureAwait(false);
        }
        catch (HttpException e)
        {
            _logger.LogWarning(e,
                "Exception on sending direct message to user {nickname} ({id}); discord error code {code} ({reason})",
                user.Username,
                user.Id,
                e.DiscordCode?.ToString("D"),
                e.Reason);
            throw new HttpRequestException($"Discord error code {e.DiscordCode?.ToString("D")} ({e.Reason})", e);
        }
    }

    public async Task UpdateGuildRolesAsync(
        ulong discordId,
        (bool IsAdvisor, bool IsClanMember, bool IsReserve) defaultRoles,
        IEnumerable<ulong>? customRoles = null,
        CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");

        RestGuildUser user = await guild.GetUserAsync(discordId, requestOptions).ConfigureAwait(false)
                             ?? throw new ArgumentOutOfRangeException(nameof(discordId), "Guild user not found");

        List<ulong> rolesToAdd = new();
        List<ulong> rolesToRemove = new();
        (bool isAdvisor, bool isClanMember, bool isReserve) = defaultRoles;
        (isReserve ? rolesToAdd : rolesToRemove).Add(_options.Value.ReserveRoleId);
        (isAdvisor ? rolesToAdd : rolesToRemove).Add(_options.Value.AdvisorRoleId);
        (isClanMember ? rolesToAdd : rolesToRemove).Add(_options.Value.ClanRoleId);


        ulong[] guildRoleIds = guild.Roles.Where(x => !x.IsManaged).Select(x => x.Id).ToArray();
        customRoles ??= Array.Empty<ulong>();
        foreach (ulong customRoleId in customRoles)
        {
            if (guildRoleIds.All(x => x != customRoleId))
            {
                _logger.LogWarning("Role with {id} not found", customRoleId);
                continue;
            }

            rolesToAdd.Add(customRoleId);
        }

        ulong[] userRoleIds = user.RoleIds.Intersect(guildRoleIds).ToArray();
        rolesToAdd = rolesToAdd.Except(userRoleIds).ToList();
        rolesToRemove = rolesToRemove.Intersect(userRoleIds).ToList();

        if (rolesToRemove.Count > 0)
            await user.RemoveRolesAsync(rolesToRemove, requestOptions).ConfigureAwait(false);
        if (rolesToAdd.Count > 0)
            await user.AddRolesAsync(rolesToAdd, requestOptions).ConfigureAwait(false);
    }

    public async Task UpdateGuildNicknameAsync(ulong discordId, string? newNickname,
        CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        RestGuildUser user =
            await DiscordClient.Rest.GetGuildUserAsync(_options.Value.GuildId, discordId, requestOptions)
                .ConfigureAwait(false)
            ?? throw new ArgumentOutOfRangeException(nameof(discordId), "User not found");
        if (user.Nickname != newNickname)
            await user.ModifyAsync(properties => { properties.Nickname = newNickname; }, requestOptions)
                .ConfigureAwait(false);
    }

    public async Task SendBotLogMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        RestGuild guild = await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId).ConfigureAwait(false)
                          ?? throw new InvalidOperationException("Clan guild not found");
        ;
        RestTextChannel channel = await guild.GetTextChannelAsync(_options.Value.LogBotChannelId).ConfigureAwait(false)
                                  ?? throw new InvalidOperationException("Log bot channel not found");
        await channel.SendMessageAsync(message).ConfigureAwait(false);
    }

    public async Task<ulong?> GetIdFromUserName(string username)
    {
        RestGuild guild = await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId).ConfigureAwait(false)
                          ?? throw new InvalidOperationException("Clan guild not found");

        string[] split = username.Split('#', StringSplitOptions.TrimEntries);
        if (split.Length == 0) return null;
        username = split[0];
        string discriminator = split.Length > 1 ? split[1] : "0000";
        RestGuildUser? user = await guild.GetUsersAsync().SelectMany(x => x.ToAsyncEnumerable())
            .FirstOrDefaultAsync(x => x.Username == username && x.Discriminator == discriminator);
        return user?.Id;
    }
}