using System.Collections.Immutable;
using AndNet.Integration.Steam;
using AndNet.Integration.Steam.Models;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Server.Options;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace AndNet.Manager.Server.Jobs;

public class ActivityStatsJob : IJob
{
    public const int IN_GAME_AUTOMATION_ID = 1;
    public const int WITH_COMRADE_AUTOMATION_ID = 2;
    private static readonly TimeSpan _dispenseInterval = TimeSpan.FromDays(7 * 3);
    private readonly Dictionary<int, int?> _baseDocumentIds;
    private readonly DatabaseContext _databaseContext;
    private readonly DocumentService _documentService;
    private readonly ILogger<ActivityStatsJob> _logger;
    private readonly SteamClient _steamClient;

    public ActivityStatsJob(
        DatabaseContext databaseContext, SteamClient steamClient, IOptions<AwardOptions> options,
        DocumentService documentService, ILogger<ActivityStatsJob> logger)
    {
        _databaseContext = databaseContext;
        _steamClient = steamClient;
        _documentService = documentService;
        _logger = logger;
        _baseDocumentIds = new()
        {
            { IN_GAME_AUTOMATION_ID, options.Value.InGameBaseDocument },
            { WITH_COMRADE_AUTOMATION_ID, options.Value.WithComradeBaseDocument }
        };
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting ActivityStatsJob…");
        DbClanPlayer? firstAdvisor = _databaseContext.ClanPlayers.First(x => x.Rank == PlayerRank.FirstAdvisor);
        DbPlayer[] players = await _databaseContext.Players
            .Include(x => x.Awards)
            .Where(x => x.SteamId.HasValue)
            .ToArrayAsync().ConfigureAwait(false);

        PlayerActivityResultNode[] allActivity = await _steamClient
            .PlayersActivityAsync(players.Select(x => x.SteamId!.Value)).ToArrayAsync().ConfigureAwait(false);

        Dictionary<ulong, int> playerIds = await _databaseContext.Players
            .ToAsyncEnumerable()
            .Join(allActivity.Select(x => x.SteamId).ToAsyncEnumerable(), x => x.SteamId, x => x,
                (player, steamId) => new { steamId, player.Id })
            .ToDictionaryAsync(x => x.steamId, x => x.Id).ConfigureAwait(false);
        Dictionary<int, PlayerStatisticsStatus> statuses = allActivity.ToDictionary(x => playerIds[x.SteamId], x =>
            x.State switch
            {
                PlayerActivityResultNode.PersonaState.Snooze => PlayerStatisticsStatus.Offline,
                _ when x.InSpaceEngineers => PlayerStatisticsStatus.InSpaceEngineers,
                _ when !x.InSpaceEngineers && !string.IsNullOrEmpty(x.GameId) => PlayerStatisticsStatus.InDifferentGame,
                PlayerActivityResultNode.PersonaState.Offline => PlayerStatisticsStatus.Offline,
                PlayerActivityResultNode.PersonaState.Online => PlayerStatisticsStatus.Online,
                PlayerActivityResultNode.PersonaState.Busy => PlayerStatisticsStatus.Online,
                PlayerActivityResultNode.PersonaState.Away => PlayerStatisticsStatus.Offline,
                PlayerActivityResultNode.PersonaState.LookingToTrade => PlayerStatisticsStatus.Online,
                PlayerActivityResultNode.PersonaState.LookingToPlay => PlayerStatisticsStatus.Online,
                null => PlayerStatisticsStatus.Offline,
                _ => throw new ArgumentOutOfRangeException()
            });

        foreach (IGrouping<ulong?, PlayerActivityResultNode> serverGroup in allActivity.Where(x => x.InSpaceEngineers)
                     .GroupBy(x => x.GameServerSteamId ?? x.LobbySteamId))
        {
            int count = 0;
            PlayerActivityResultNode? firstNode = null;
            foreach (PlayerActivityResultNode activity in serverGroup)
            {
                await GiveAward(activity, IN_GAME_AUTOMATION_ID, "Запуск игры").ConfigureAwait(false);
                count++;
                if (serverGroup.Key is null) continue;
                if (count > 1)
                {
                    if (firstNode is not null)
                        await GiveAward(firstNode.Value, WITH_COMRADE_AUTOMATION_ID, "Совместная игра")
                            .ConfigureAwait(false);

                    firstNode = null;
                    await GiveAward(activity, WITH_COMRADE_AUTOMATION_ID, "Совместная игра").ConfigureAwait(false);
                }
                else
                {
                    firstNode ??= activity;
                }
            }
        }

        DateTime fireTime = context?.FireTimeUtc.UtcDateTime ?? DateTime.UtcNow;
        await _databaseContext.PlayerStats.AddRangeAsync(statuses.Select(x => new DbPlayerStat
        {
            PlayerId = x.Key,
            Date = fireTime,
            Status = x.Value
        })).ConfigureAwait(false);

        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        _logger.LogInformation("ActivityStatsJob is done");

        async Task GiveAward(PlayerActivityResultNode activity, int automationId, string description)
        {
            int playerId = playerIds[activity.SteamId];
            if (!await _databaseContext.ClanPlayers.AnyAsync(x => x.Id == playerId && x.Rank < PlayerRank.Advisor)
                    .ConfigureAwait(false)) return;
            if (automationId == WITH_COMRADE_AUTOMATION_ID)
                statuses[playerId] = PlayerStatisticsStatus.InSpaceEngineersWithComrade;
            DateTime lastDate;
            try
            {
                lastDate = await _databaseContext.Awards
                    .Where(x => x.PlayerId == playerId && x.AutomationId == automationId).MaxAsync(x => x.IssueDate)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                lastDate = DateTime.MinValue;
            }

            if (DateTime.UtcNow - lastDate < _dispenseInterval) return;

            DbDoc doc = new()
            {
                AuthorId = firstAdvisor.Id,
                CreationDate = DateTime.UtcNow,
                Author = firstAdvisor,
                Info = new DecisionCouncilPlayerAwardSheet
                {
                    Action = DecisionCouncilPlayer.PlayerAction.Generic,
                    Description = description,
                    PlayerId = playerId,
                    AwardType = AwardType.Copper,
                    AutomationId = automationId,
                    MinYesVotesPercent = AwardRules.MinCouncilVotes[AwardType.Copper],
                    Votes = ImmutableList<Decision.Vote>.Empty
                },
                Body = new()
                {
                    Body = @"# Наградной лист

Награда выдана автоматически."
                },
                ParentId = _baseDocumentIds[automationId]
            };
            doc.GenerateTitleFromBody();
            await _databaseContext.Documents.AddAsync(doc).ConfigureAwait(false);
            await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
            await _documentService.AgreeExecuteAsync(doc, firstAdvisor).ConfigureAwait(false);
            _databaseContext.Update(doc).State = EntityState.Modified;
        }
    }
}