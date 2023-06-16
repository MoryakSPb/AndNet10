/*using AndNet.Integration.Steam.Client.Models;
using AndNet.Manager.Database;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Registry.Worker.Jobs;

public class CalcPlayersJob : IJob
{
    private readonly RegistryDatabaseContext _databaseContext;

    public CalcPlayersJob(RegistryDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await Task.WhenAll((
                await _databaseContext.ClanPlayers.Select(x => x.Id).ToArrayAsync().ConfigureAwait(false))
            .Select(x => _calcPlayerClient.GetResponse<CalcPlayerResponse>(new() { PlayerId = x }))).ConfigureAwait(false);
    }
}*/

