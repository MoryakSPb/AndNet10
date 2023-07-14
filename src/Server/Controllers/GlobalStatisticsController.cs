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
    private static (DateTime date, GlobalStats stats) _stats = (DateTime.MinValue, new());
    private readonly DatabaseContext _databaseContext;

    public GlobalStatisticsController(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    [HttpGet]
    public async Task<ActionResult<GlobalStats>> Get()
    {
        DateTime date = DateTime.UtcNow.Date.AddDays(-30);
        string eTag = $"W/\"{date.DayOfYear:D}\"";
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = eTag;
        if (_stats.date == date) return _stats.stats;

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
        _stats = (date, result);
        return Ok(result);
    }
}