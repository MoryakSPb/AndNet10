using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "member")]
[ApiController]
public class PlayerController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly UserManager<DbUser> _userManager;

    public PlayerController(DatabaseContext context, UserManager<DbUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async IAsyncEnumerable<Player> GetPlayers([FromQuery] int skip = 0,
        [FromQuery] int take = int.MaxValue,
        [FromQuery] bool onlyClanMembers = true,
        [FromQuery] string? search = "")
    {
        IQueryable<DbPlayer> set =
            onlyClanMembers
                ? _context.ClanPlayers
                    .OrderByDescending(x => x.Rank)
                    .ThenByDescending(x => x.Score)
                    .ThenBy(x => x.JoinDate)
                    .ThenBy(x => x.DetectionDate)
                    .AsNoTracking()
                : _context.Players.OrderBy(x => x.DetectionDate).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            set = set.Where(x => EF.Functions.ILike(x.Nickname + " " + x.RealName, $"%{search}%"));
        HttpContext.Response.Headers["Items-Count"] =
            (await set.CountAsync().ConfigureAwait(false)).ToString("D");
        foreach (DbPlayer player in set.Skip(skip).Take(take))
            switch (player)
            {
                case DbClanPlayer clanPlayer:
                    yield return (ClanPlayer)clanPlayer;
                    continue;
                case DbFormerClanPlayer formerClanPlayer:
                    yield return (FormerClanPlayer)formerClanPlayer;
                    continue;
                case DbExternalPlayer externalPlayer:
                    yield return (ExternalPlayer)externalPlayer;
                    continue;
            }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClanPlayer), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FormerClanPlayer), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExternalPlayer), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Player>> GetDbPlayer(int id)
    {
        DbPlayer? dbPlayer = await _context.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
        return dbPlayer switch
        {
            null => NotFound(),
            DbClanPlayer clanPlayer => (ClanPlayer)clanPlayer,
            DbFormerClanPlayer formerClanPlayer => (FormerClanPlayer)formerClanPlayer,
            DbExternalPlayer externalPlayer => (ExternalPlayer)externalPlayer,
            _ => throw new NotSupportedException()
        };
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ClanPlayer), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClanPlayer>> GetMe()
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return NotFound();
        DbClanPlayer? player = await _context.ClanPlayers.FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false);
        if (player is null) return NotFound();
        return (ClanPlayer)player;
    }
}