using AndNet.Manager.Database;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class GlobalStatisticsController : ControllerBase
{
    private static (DateTime createTime, GlobalStats cached) _globalStatsCached = (DateTime.MinValue, null!);
    private readonly DatabaseContext _databaseContext;

    public GlobalStatisticsController(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    [HttpGet]
    [ResponseCache(Duration = 10800, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<GlobalStats>> Get()
    {
        if (DateTime.UtcNow - _globalStatsCached.createTime < TimeSpan.FromHours(3))
            return Ok(_globalStatsCached.cached);

        DateTime date = DateTime.UtcNow.Date.AddDays(-30);
        GlobalStats result = new()
        {
            AwardsIssued = await _databaseContext.Awards.CountAsync(x => x.IssueDate >= date).ConfigureAwait(false),
            Battles = await _databaseContext.Documents.CountAsync(x => x.CreationDate >= date
                                                                       && x.Info!.Type == "ОБ").ConfigureAwait(false),
            NewPlayers = await _databaseContext.Documents.CountAsync(x => x.CreationDate >= date
                                                                          && x.Info!.Type == "РCИП"
                                                                          && EF.Functions.JsonContains(x.Info,
                                                                              @"{""IsExecuted"": true}"))
                .ConfigureAwait(false),
            HoursPlayed = (int)Math.Truncate(0.25 * await _databaseContext.PlayerStats
                .Include(x => x.Player)
                .Where(x => x.Player.Status == PlayerStatus.Member && x.Date >= date
                                                                   && x.Status >= PlayerStatisticsStatus
                                                                       .InSpaceEngineers)
                .CountAsync()
                .ConfigureAwait(false)),
            MaxInGame = await _databaseContext.PlayerStats
                .Include(x => x.Player)
                .Where(x => x.Player.Status == PlayerStatus.Member && x.Date >= date
                                                                   && x.Status >= PlayerStatisticsStatus
                                                                       .InSpaceEngineers)
                .GroupBy(x => x.Date)
                .Select(x => x.Count())
                .MaxAsync().ConfigureAwait(false),
            MaxOnline = await _databaseContext.PlayerStats
                .Include(x => x.Player)
                .Where(x => x.Player.Status == PlayerStatus.Member && x.Date >= date
                                                                   && x.Status >= PlayerStatisticsStatus.Online)
                .GroupBy(x => x.Date)
                .Select(x => x.Count())
                .MaxAsync().ConfigureAwait(false)
        };
        _globalStatsCached = (DateTime.UtcNow, result);
        return Ok(result);
    }
}