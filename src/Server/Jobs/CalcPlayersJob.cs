using System.Text;
using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs;

public class CalcPlayersJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;
    private readonly ILogger<CalcPlayersJob> _logger;

    public CalcPlayersJob(DatabaseContext databaseContext, ILogger<CalcPlayersJob> logger,
        DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _logger = logger;
        _discordService = discordService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Dictionary<ulong, (PlayerRank oldRank, PlayerRank newRank)> ranks = new(64);
        _logger.LogInformation("Starting CalcPlayersJob…");
        foreach (DbClanPlayer clanPlayer in _databaseContext.ClanPlayers.Include(x => x.Awards))
        {
            PlayerRank oldRank = clanPlayer.Rank;
            clanPlayer.CalcPlayer();
            PlayerRank newRank = clanPlayer.Rank;
            if (clanPlayer.DiscordId is not null) ranks.Add(clanPlayer.DiscordId.Value, (oldRank, newRank));
        }

        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (DbClanPlayer clanPlayer in _databaseContext.ClanPlayers.Include(x => x.Expeditions))
        {
            await _discordService.UpdateGuildNicknameAsync(clanPlayer.DiscordId!.Value, clanPlayer.ToString())
                .ConfigureAwait(false);
            await _discordService.UpdateGuildRolesAsync(clanPlayer.DiscordId!.Value,
                    (clanPlayer.Rank >= PlayerRank.Advisor,
                        true,
                        clanPlayer.OnReserve),
                    clanPlayer.Expeditions.Where(x => x.DiscordRoleId is not null).Select(x => x.DiscordRoleId!.Value))
                .ConfigureAwait(false);
        }

        StringBuilder text = new();
        foreach (KeyValuePair<ulong, (PlayerRank oldRank, PlayerRank newRank)> pair in
                 ranks.Where(x => x.Value.oldRank != x.Value.newRank))
            text.AppendLine(
                $"<@{pair.Key:D}> {(pair.Value.newRank > pair.Value.oldRank ? "повышен(а)" : "понижен(а)")} в ранге до [{RankRules.Icons[pair.Value.newRank]}] «{RankRules.Names[pair.Value.newRank]}»");

        if (text.Length > 0) await _discordService.SendBotLogMessageAsync(text.ToString()).ConfigureAwait(false);
        _logger.LogInformation("CalcPlayersJob is done");
    }
}