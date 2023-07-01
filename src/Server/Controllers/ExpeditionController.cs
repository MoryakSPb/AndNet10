using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "member")]
[ApiController]
public class ExpeditionController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly UserManager<DbUser> _userManager;

    public ExpeditionController(DatabaseContext context, UserManager<DbUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<Expedition>), StatusCodes.Status200OK)]
    public async IAsyncEnumerable<Expedition> GetExpeditions([FromQuery] int skip = 0,
        [FromQuery] int? playerId = null,
        [FromQuery] int take = int.MaxValue,
        [FromQuery] bool getDeleted = false)
    {
        IQueryable<DbExpedition> set =
            (getDeleted
                ? _context.Expeditions.IgnoreQueryFilters()
                : _context.Expeditions).Include(x => x.Members).AsNoTracking();
        if (playerId is not null) set = set.Where(x => x.Members.Any(y => y.Id == playerId));
        HttpContext.Response.Headers["Items-Count"] =
            (await set.CountAsync().ConfigureAwait(false)).ToString("D");
        foreach (DbExpedition expedition in set.Skip(skip).Take(take)) yield return (Expedition)expedition;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Expedition), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Expedition>> GetExpedition([FromRoute] int id)
    {
        IQueryable<DbExpedition> set = _context.Expeditions
            .Include(x => x.Members)
            .IgnoreQueryFilters()
            .AsNoTracking();
        DbExpedition? dbExpedition = await set.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (dbExpedition is null) return NotFound();
        return Ok((Expedition)dbExpedition);
    }

    [HttpGet("{id}/docs")]
    [ProducesResponseType(typeof(Doc[]), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 150, Location = ResponseCacheLocation.Client)]
    public async IAsyncEnumerable<Doc> GetDbPlayerDocs([FromRoute] int id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = int.MaxValue)
    {
        string json = $"{{\"{nameof(DecisionCouncilExpedition.ExpeditionId)}\": {id}}}";
        IOrderedQueryable<DbDoc> set = _context.Documents.AsNoTracking()
            .Where(x => x.Info != null && EF.Functions.JsonContains(x.Info, json))
            .OrderByDescending(x => x.CreationDate);
        HttpContext.Response.Headers["Items-Count"] =
            (await set.CountAsync().ConfigureAwait(false)).ToString("D");
        foreach (DbDoc dbDoc in set.Skip(skip).Take(Math.Min(take, 100))) yield return dbDoc;
    }

    [HttpPatch("{id:int}/join")]
    [ProducesResponseType(typeof(Expedition), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Expedition>> JoinToExpedition([FromRoute] int id)
    {
        IQueryable<DbExpedition> set = _context.Expeditions
            .Include(x => x.Members)
            .IgnoreQueryFilters()
            .AsNoTracking();
        DbExpedition? dbExpedition = await set.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (dbExpedition is null) return NotFound();

        DbClanPlayer? player = await GetClanPlayer().ConfigureAwait(false);
        if (player is null) return NotFound();

        if (dbExpedition.Members.Any(x => x.Id == player.Id)) return Conflict();

        await _context.Documents.AddAsync(new()
        {
            Author = player,
            CreationDate = DateTime.UtcNow,
            AuthorId = player.Id,
            Info = new DecisionCouncilExpeditionPlayer
            {
                Action = DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Add,
                PlayerId = player.Id,
                ExpeditionId = dbExpedition.Id
            }.GenerateVotes(_context),
            Body = new()
            {
                Body =
                    $"# Заявка о присоединении к экспедиции{Environment.NewLine}Игрок {player.Nickname} запрашивает получение статуса участника экспедиции №{dbExpedition.Id}"
            }
        }).ConfigureAwait(false);

        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    [HttpPatch("{id:int}/leave")]
    [ProducesResponseType(typeof(Expedition), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Expedition>>? LeaveFromExpedition([FromRoute] int id)
    {
        IQueryable<DbExpedition> set = _context.Expeditions
            .Include(x => x.Members)
            .IgnoreQueryFilters()
            .AsNoTracking();
        DbExpedition? dbExpedition = await set.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (dbExpedition is null) return NotFound();

        DbClanPlayer? player = await GetClanPlayer().ConfigureAwait(false);
        if (player is null) return NotFound();

        if (dbExpedition.AccountablePlayerId == player.Id || dbExpedition.Members.All(x => x.Id != player.Id))
            return Conflict();

        await _context.Documents.AddAsync(new DbDoc
        {
            Author = player,
            CreationDate = DateTime.UtcNow,
            AuthorId = player.Id,
            Info = new DecisionCouncilExpeditionPlayer
            {
                Action = DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Remove,
                PlayerId = player.Id,
                ExpeditionId = dbExpedition.Id
            }.GenerateVotes(_context),
            Body = new()
            {
                Body =
                    $"# Заявка о исключении из состава экспедиции{Environment.NewLine}Игрок {player.Nickname} запрашивает исключение себя из состава экспедиции №{dbExpedition.Id}"
            }
        }.GenerateTitleFromBody()).ConfigureAwait(false);

        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    [HttpPatch("{id:int}/close")]
    [ProducesResponseType(typeof(Expedition), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseExpedition([FromRoute] int id)
    {
        throw new NotImplementedException();
    }

    [HttpPatch("{id:int}/prolong")]
    [ProducesResponseType(typeof(Expedition), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProlongExpedition([FromRoute] int id)
    {
        throw new NotImplementedException();
    }

    [HttpPatch("{id:int}/transfer")]
    [ProducesResponseType(typeof(Expedition), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferCommanderExpedition([FromRoute] int id)
    {
        throw new NotImplementedException();
    }

    private async Task<DbClanPlayer?> GetClanPlayer()
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return null;
        DbClanPlayer? player = await _context.ClanPlayers.FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false);
        return player;
    }
}