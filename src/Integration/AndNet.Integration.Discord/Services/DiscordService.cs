using System.Runtime.CompilerServices;
using AndNet.Integration.Discord.Options;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AndNet.Integration.Discord.Services;

public class DiscordService : IHostedService, IDisposable
{
    private const ulong _ADVISOR_PERMISSIONS = 0b100010101111110000011011110111110111111000000ul;
    private const ulong _MEMBER_PERMISSIONS = 0b101110110000000001100011100111001000000ul;
    private const ulong _EXPEDITION_COMMANDER_PERMISSIONS = 0b101110110000011111100111110111101000000ul;
    private const ulong _EXPEDITION_MEMBER_PERMISSIONS = 0b101110110000010001100011100111001000000ul;

    private readonly ILogger<DiscordService> _logger;
    private readonly IOptions<DiscordOptions> _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public DiscordService(IOptions<DiscordOptions> options, ILogger<DiscordService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public static bool SilentMode { get; set; } = false;

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
        await DiscordClient.SetActivityAsync(new Game("andromeda-se.xyz", ActivityType.Listening))
            .ConfigureAwait(false);
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
        /*if (SilentMode)
        {
            _logger.LogInformation("SILENT MODE! Bot direct message to «{nickname}» [{discordId:D}]: {message}",
                user.Username, discordId, message);
            return;
        }*/

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
        RestGuildUser user;
        try
        {
            user = await guild.GetUserAsync(discordId, requestOptions).ConfigureAwait(false);
            if (user is null) throw new ArgumentOutOfRangeException(nameof(discordId));
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogWarning("User with id {id} not found in guild", discordId);
            return;
        }

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
        RestGuildUser user;

        try
        {
            user =
                await DiscordClient.Rest.GetGuildUserAsync(_options.Value.GuildId, discordId, requestOptions)
                    .ConfigureAwait(false);
            if (user is null) throw new ArgumentOutOfRangeException(nameof(discordId));
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogWarning("User with id {id} not found in guild", discordId);
            return;
        }


        if (user.Nickname != newNickname)
            await user.ModifyAsync(properties => { properties.Nickname = newNickname; }, requestOptions)
                .ConfigureAwait(false);
    }

    public async Task<ulong> CreateExpeditionRoleAsync(int number, CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");

        RestRole? result = await guild
            .CreateRoleAsync($"Экспедиция №{number:D}", new GuildPermissions(0ul), null, true, true, requestOptions)
            .ConfigureAwait(false);

        AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using ConfiguredAsyncDisposable _ = scope.ConfigureAwait(false);
        DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        await guild.ReorderRolesAsync(Enumerable.Empty<ReorderRoleProperties>()
                .Append(new(_options.Value.AdvisorRoleId, 100))
                .Concat(database.Expeditions
                    .OrderBy(x => x.Id)
                    .Where(x => x.DiscordRoleId.HasValue)
                    .Select(x => new { x.Id, x.DiscordRoleId })
                    .ToArray()
                    .Select(x => new ReorderRoleProperties(x.DiscordRoleId!.Value, 1000 + x.Id)))
                .Append(new(_options.Value.ReserveRoleId, 2001))
                .Append(new(_options.Value.ClanRoleId, 3000))
            , requestOptions).ConfigureAwait(false);
        return result.Id;
    }

    public async Task RemoveRole(ulong id, CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");

        RestRole? role = guild.GetRole(id);
        if (role is not null) await role.DeleteAsync(requestOptions).ConfigureAwait(false);
    }

    public async Task SendBotLogMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (SilentMode)
        {
            _logger.LogInformation("SILENT MODE! Bot log message: {message}", message);
            return;
        }

        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");
        RestTextChannel channel =
            await guild.GetTextChannelAsync(_options.Value.LogBotChannelId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Log bot channel not found");
        var result = await channel.SendMessageAsync(message, options: requestOptions).ConfigureAwait(false);
        await result.ModifyAsync(properties =>
        {
            properties.Embeds = new(Array.Empty<Embed>());
        }).ConfigureAwait(false);
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
            .FirstOrDefaultAsync(x => x.Username == username && x.Discriminator == discriminator).ConfigureAwait(false);
        return user?.Id;
    }

    public async Task SendNoReaction(IAsyncEnumerable<ulong> targets, ulong channelId, ulong messageId, string text,
        CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");
        RestTextChannel channel = await guild.GetTextChannelAsync(channelId, requestOptions).ConfigureAwait(false)
                                  ?? throw new ArgumentException("Channel not found", nameof(channelId));
        RestMessage message = await channel.GetMessageAsync(messageId, requestOptions).ConfigureAwait(false)
                              ?? throw new ArgumentException("Channel not found", nameof(messageId));

        await foreach (ulong discordId in targets
                           .Except(
                               message.Reactions.ToAsyncEnumerable()
                                   .SelectMany(x =>
                                       message.GetReactionUsersAsync(x.Key, x.Value.ReactionCount, requestOptions))
                                   .SelectMany(x => x.ToAsyncEnumerable())
                                   .Select(x => x.Id))
                           .Distinct()
                           .ConfigureAwait(false))
            try
            {
                await SendDirectMessageAsync(discordId, text, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
            }
    }

    public async Task CreateExpeditionsChannels(int expeditionId, CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using ConfiguredAsyncDisposable _ = scope.ConfigureAwait(false);
        DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        DbExpedition? expedition = database.Expeditions
                                       .Include(x => x.AccountablePlayer)
                                       .FirstOrDefault(x => x.Id == expeditionId)
                                   ?? throw new ArgumentException(nameof(expeditionId));
        ulong roleId = expedition.DiscordRoleId!.Value;

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");

        await guild.CreateForumChannelAsync($"экспедиция-{expeditionId:D}", properties =>
        {
            properties.DefaultSortOrder = new(ForumSortOrder.LatestActivity);
            properties.DefaultLayout = new(ForumLayout.List);
            properties.DefaultReactionEmoji = new(new Emoji("👍"));
            properties.AutoArchiveDuration = new(ThreadArchiveDuration.OneWeek);
            properties.CategoryId = _options.Value.ExpeditionsCategoryId;
            properties.Position = (10000 + expeditionId) << 1;
            properties.PermissionOverwrites = new(new[]
            {
                new Overwrite(roleId,
                    PermissionTarget.Role,
                    new(_EXPEDITION_MEMBER_PERMISSIONS, ~_EXPEDITION_MEMBER_PERMISSIONS))
            });
        }).ConfigureAwait(false);
        await guild.CreateVoiceChannelAsync($"Канал Э{expeditionId:D}", properties =>
        {
            properties.CategoryId = _options.Value.ExpeditionsCategoryId;
            properties.Position = (10000 + expeditionId) << (1 + 1);
            properties.PermissionOverwrites = new(new[]
            {
                new Overwrite(roleId,
                    PermissionTarget.Role,
                    new(_EXPEDITION_MEMBER_PERMISSIONS, ~_EXPEDITION_MEMBER_PERMISSIONS))
            });
        }).ConfigureAwait(false);
        //await ChangeCommanderPermissions(expeditionId, cancellationToken).ConfigureAwait(false);
    }

    public async Task ChangeCommanderPermissions(int expeditionId, CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = RequestOptions.Default;
        requestOptions.CancelToken = cancellationToken;

        AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using ConfiguredAsyncDisposable _ = scope.ConfigureAwait(false);
        DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        DbExpedition? expedition = database.Expeditions
                                       .Include(x => x.AccountablePlayer)
                                       .FirstOrDefault(x => x.Id == expeditionId)
                                   ?? throw new ArgumentException(nameof(expeditionId));

        RestGuild guild =
            await DiscordClient.Rest.GetGuildAsync(_options.Value.GuildId, requestOptions).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clan guild not found");
    }
}