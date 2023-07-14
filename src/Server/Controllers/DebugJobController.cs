using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Server.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndNet.Manager.Server.Controllers;

#if DEBUG
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "advisor")]
public class DebugJobController : ControllerBase
{
    private readonly CalcPlayersJob _calcPlayersJob;
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;
    private readonly ActivityStatsJob _gameAwardJob;

    public DebugJobController(CalcPlayersJob calcPlayersJob, ActivityStatsJob gameAwardJob,
        DiscordService discordService, DatabaseContext databaseContext)
    {
        _calcPlayersJob = calcPlayersJob;
        _gameAwardJob = gameAwardJob;
        _discordService = discordService;
        _databaseContext = databaseContext;
    }

    [HttpGet(nameof(CalcPlayers))]
    public async Task<IActionResult> CalcPlayers()
    {
        await _calcPlayersJob.Execute(null!).ConfigureAwait(false);
        return Ok();
    }


    [HttpGet(nameof(InGameAward))]
    public async Task<IActionResult> InGameAward()
    {
        await _gameAwardJob.Execute(null!).ConfigureAwait(false);
        return Ok();
    }

    [HttpPost(nameof(SendNoReaction))]
    public async Task<IActionResult> SendNoReaction([FromQuery] ulong channelId, [FromQuery] ulong messageId,
        [FromBody] string message)
    {
        await _discordService.SendNoReaction(_databaseContext.ClanPlayers
            .OrderByDescending(x => x.Rank)
            .ThenByDescending(x => x.Score)
            .ThenBy(x => x.JoinDate)
            .ThenBy(x => x.Nickname)
            .Where(x => x.DiscordId.HasValue)
            .Select(x => x.DiscordId)
            .ToAsyncEnumerable()
            .Select(x => x!.Value), channelId, messageId, message).ConfigureAwait(false);
        return Ok();
    }
}
#endif