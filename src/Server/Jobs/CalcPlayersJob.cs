using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Player;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs;

public class CalcPlayersJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<CalcPlayersJob> _logger;

    public CalcPlayersJob(DatabaseContext databaseContext, ILogger<CalcPlayersJob> logger)
    {
        _databaseContext = databaseContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting CalcPlayersJob…");
        foreach (DbClanPlayer clanPlayer in _databaseContext.ClanPlayers.Include(x => x.Awards))
            clanPlayer.CalcPlayer();
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        _logger.LogInformation("CalcPlayersJob is done");
    }
}