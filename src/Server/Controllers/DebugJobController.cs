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
    private readonly ActivityStatsJob _gameAwardJob;

    public DebugJobController(CalcPlayersJob calcPlayersJob, ActivityStatsJob gameAwardJob)
    {
        _calcPlayersJob = calcPlayersJob;
        _gameAwardJob = gameAwardJob;
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
}
#endif