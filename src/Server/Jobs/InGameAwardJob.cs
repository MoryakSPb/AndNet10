/*using AndNet.Integration.Steam.Client.Models;
using AndNet.Registry.Abstractions;
using AndNet.Registry.Abstractions.Enums;
using AndNet.Registry.Client.Models.Award;
using AndNet.Registry.Database;
using AndNet.Registry.Worker.Database;
using AndNet.Registry.Worker.Database.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Registry.Worker.Jobs;

public class InGameAwardJob : IJob
{
    public const int IN_GAME_AUTOMATION_ID = 1;
    public const int WITH_COMRADE_AUTOMATION_ID = 2;
    private static readonly TimeSpan _dispenseInterval = TimeSpan.FromDays(7 * 3);
    private readonly IRequestClient<PlayersActivityRequest> _activityClient;
    private readonly IRequestClient<CreateAwardRequest> _createAwardClient;
    private readonly RegistryDatabaseContext _databaseContext;
    private readonly RegistryWorkerDatabaseContext _workerDatabaseContext;

    public InGameAwardJob(
        IRequestClient<PlayersActivityRequest> activityClient,
        IRequestClient<CreateAwardRequest> createAwardClient, 
        RegistryDatabaseContext databaseContext,
        RegistryWorkerDatabaseContext workerDatabaseContext)
    {
        _activityClient = activityClient;
        _createAwardClient = createAwardClient;
        _databaseContext = databaseContext;
        _workerDatabaseContext = workerDatabaseContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var players = await _databaseContext.ClanPlayers
            .Where(x => x.SteamId.HasValue)
            .Select(x => new { x.Id, x.SteamId})
            .ToArrayAsync().ConfigureAwait(false);
        Response<PlayersActivityResponse> activitiesResponse = await _activityClient
            .GetResponse<PlayersActivityResponse>(new()
            {
                SteamIds = players.Select(x => x.SteamId!.Value).ToArray()
            }).ConfigureAwait(false);

        IReadOnlyCollection<PlayersActivityResponseNode> playerActivities = activitiesResponse.Message.PlayersActivities;
        foreach (PlayersActivityResponseNode activity in playerActivities.Where(x => x.InSpaceEngineers))
        {
            await GiveAward(activity, IN_GAME_AUTOMATION_ID, "Запуск игры").ConfigureAwait(false);
        }

        foreach (IGrouping<ulong?, PlayersActivityResponseNode> serverGroup in
                 playerActivities.GroupBy(x => x.GameServerSteamId).Where(x => x.Key is not null)
                     .Concat(
                         playerActivities.GroupBy(x => x.LobbySteamId).Where(x => x.Key is not null)))
        {
            PlayersActivityResponseNode[] activities = serverGroup.ToArray();
            if (activities.Length < 2) continue;

            foreach (PlayersActivityResponseNode activity in activities)
            {
                await GiveAward(activity, WITH_COMRADE_AUTOMATION_ID, "Совместная игра").ConfigureAwait(false);
            }
        }

        

        async Task GiveAward(PlayersActivityResponseNode activity, int automationId, string description)
        {
            var player = players.First(x => x.SteamId == activity.SteamId);
            DateTime lastDate;
            try
            {
                lastDate = _workerDatabaseContext.AwardPlayerInfos
                    .Where(x => x.AutomationId == automationId && x.PlayerId == player.Id)
                    .Max(x => x.SuccessRunDate);
            }
            catch (InvalidOperationException)
            {
                lastDate = DateTime.MinValue;
            }

            if (DateTime.UtcNow - lastDate < _dispenseInterval) return;
            Response<CreateAwardResponse> result = await _createAwardClient.GetResponse<CreateAwardResponse>(new()
            {
                PlayerId = player.Id,
                IssuerId = null,
                Type = AwardType.Copper,
                Description = description
            }).ConfigureAwait(false);
            if (result.Message.Id.HasValue)
            {
                await _workerDatabaseContext.AwardPlayerInfos.AddAsync(new()
                {
                    AutomationId = automationId,
                    PlayerId = player.Id,
                    SuccessRunDate = DateTime.UtcNow
                }).ConfigureAwait(false);
                await _workerDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}*/

