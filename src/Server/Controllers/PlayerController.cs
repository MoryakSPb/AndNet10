using System.ComponentModel.DataAnnotations;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        if (dbPlayer is null) return NotFound();
        string eTag = $"W/\"{dbPlayer.Version:D10}\"";
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = eTag;
        return dbPlayer switch
        {
            DbClanPlayer clanPlayer => (ClanPlayer)clanPlayer,
            DbFormerClanPlayer formerClanPlayer => (FormerClanPlayer)formerClanPlayer,
            DbExternalPlayer externalPlayer => (ExternalPlayer)externalPlayer,
            _ => throw new NotSupportedException()
        };
    }

    [HttpGet("{id:int}/nickname")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<string>> GetDbPlayerNickname(int id)
    {
        DbPlayer? dbPlayer = await _context.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
        if (dbPlayer is null) return NotFound();
        return Ok(dbPlayer.ToString());
    }

    [HttpGet("{id:int}/stats")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<IReadOnlyDictionary<DateTime, PlayerStatisticsStatus>>> GetDbPlayerStats(
        [FromRoute] int id)
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return Unauthorized();
        DbClanPlayer? player = await _context.ClanPlayers.Include(x => x.Expeditions)
            .FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false);
        if (player is null) return Unauthorized();

        DbPlayer? target = await _context.Players.Include(x => x.Expeditions).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (target is null) return NotFound();
        if (player.Rank < PlayerRank.Advisor)
        {
            if (player.Rank == PlayerRank.Penal && target.Id != player.Id) return Forbid();
            PlayerRank[] ranks = Enum.GetValues<PlayerRank>();
            PlayerRank nextRank = ranks[Array.IndexOf(ranks, player.Rank) + 1];
            if (target is DbClanPlayer clanTarget && clanTarget.Rank > nextRank) return Forbid();
        }

        return await _context.PlayerStats.AsNoTracking()
            .Where(x => x.PlayerId == id)
            .OrderBy(x => x.Date)
            .ToDictionaryAsync(x => x.Date, x => x.Status).ConfigureAwait(false);
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
        string eTag = $"W/\"{player.Version:D10}\"";
        Response.Headers.ETag = eTag;
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        return (ClanPlayer)player;
    }

    [HttpPatch("me")]
    [ProducesResponseType(typeof(void), StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(void), StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClanPlayer>> PatchMe([FromBody] PlayerPatch patch)
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return NotFound();
        DbClanPlayer? player = await _context.ClanPlayers.FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false);
        if (player is null) return NotFound();

        if (patch.Nickname is not null && player.Nickname != patch.Nickname)
        {
            if (string.IsNullOrWhiteSpace(patch.Nickname)) return BadRequest();
            await _context.Documents.AddAsync(new()
            {
                Author = player,
                AuthorId = player.Id,
                Title = "Обращение к совету о смене никнейма",
                CreationDate = DateTime.UtcNow,
                Body = new()
                {
                    Body = $@"# Обращение к совету о смене никнейма

Обращаюсь к совету с просьбой изменить мой **никнейм** с «{player.Nickname}» на «{patch.Nickname}»"
                },
                Info = new DecisionCouncilPlayerChange
                {
                    Action = DecisionCouncilPlayer.PlayerAction.Generic,
                    MinYesVotesPercent = 0.5,
                    PlayerId = player.Id,
                    Property = DecisionCouncilPlayerChange.PlayerChangeProperty.Nickname,
                    NewValue = patch.Nickname
                }.GenerateVotes(_context)
            }).ConfigureAwait(false);
        }

        if (patch.RealName is not null && player.RealName != patch.RealName)
        {
            player.RealName = string.IsNullOrWhiteSpace(player.RealName) ? null : patch.RealName;
            if (string.IsNullOrWhiteSpace(patch.Nickname)) return BadRequest();
            await _context.Documents.AddAsync(new()
            {
                Author = player,
                AuthorId = player.Id,
                Title = "Обращение к совету о смене имени",
                CreationDate = DateTime.UtcNow,
                Body = player.RealName is null
                    ? new()
                    {
                        Body = @"# Обращение к совету о смене имени

Обращаюсь к совету с просьбой *убрать* упоминание моего **имени** на ресурсах клана"
                    }
                    : new()
                    {
                        Body = $@"# Обращение к совету о смене имени

Обращаюсь к совету с просьбой изменить мое **имя** с «{player.RealName}» на «{patch.RealName}»"
                    },
                Info = new DecisionCouncilPlayerChange
                {
                    Action = DecisionCouncilPlayer.PlayerAction.Generic,
                    MinYesVotesPercent = 0.5,
                    PlayerId = player.Id,
                    Property = DecisionCouncilPlayerChange.PlayerChangeProperty.RealName,
                    NewValue = patch.RealName
                }.GenerateVotes(_context)
            }).ConfigureAwait(false);
        }

        if (patch.TimeZone is not null && player.TimeZone?.Id != patch.TimeZone)
        {
            if (string.IsNullOrEmpty(patch.TimeZone)) player.TimeZone = null;
            try
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(patch.TimeZone);
                player.TimeZone = timeZoneInfo;
            }
            catch (TimeZoneNotFoundException)
            {
                return BadRequest();
            }
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    [HttpGet("{id}/docs")]
    [ProducesResponseType(typeof(Doc[]), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 150, Location = ResponseCacheLocation.Client)]
    public async IAsyncEnumerable<Doc> GetDbPlayerDocs([FromRoute] int id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = int.MaxValue)
    {
        string json = $"{{\"{nameof(DecisionCouncilPlayer.PlayerId)}\": {id}}}";
        IOrderedQueryable<DbDoc> set = _context.Documents.AsNoTracking()
            .Where(x => x.Info != null && EF.Functions.JsonContains(x.Info, json))
            .OrderByDescending(x => x.CreationDate);
        HttpContext.Response.Headers["Items-Count"] =
            (await set.CountAsync().ConfigureAwait(false)).ToString("D");
        foreach (DbDoc dbDoc in set.Skip(skip).Take(Math.Min(take, 100))) yield return dbDoc;
    }

    [Authorize(Roles = "advisor")]
    [HttpPatch("{id}/award")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> PatchAward([FromRoute] int id, [FromQuery] AwardType awardType,
        [FromQuery] [MinLength(1)] string description)
    {
        DbClanPlayer? caller = await GetCaller().ConfigureAwait(false);
        if (caller is null) return Unauthorized();
        if (!await _context.Players.AnyAsync(x => x.Id == id).ConfigureAwait(false)) return NotFound();
        EntityEntry<DbDoc> result = await _context.Documents.AddAsync(new DbDoc
        {
            Author = caller,
            AuthorId = caller.Id,
            CreationDate = DateTime.UtcNow,
            Info = new DecisionCouncilPlayerAwardSheet
            {
                PlayerId = id,
                Description = description,
                Action = DecisionCouncilPlayer.PlayerAction.Generic,
                AutomationId = null,
                AwardType = awardType,
                MinYesVotesPercent = AwardRules.MinCouncilVotes[awardType]
            }.GenerateVotes(_context),
            Body = new()
            {
                Body = "# Наградной лист"
            }
        }.GenerateTitleFromBody()).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(result.Entity.Id);
    }

    [HttpPatch("me/reserve")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> PatchReserve()
    {
        return await PatchReserve((await GetCaller().ConfigureAwait(false))!.Id).ConfigureAwait(false);
    }

    [HttpDelete("me")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> Kick()
    {
        return await Kick((await GetCaller().ConfigureAwait(false))!.Id, PlayerLeaveReason.AtWill)
            .ConfigureAwait(false);
    }

    [Authorize(Roles = "advisor")]
    [HttpPatch("{id}/reserve")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> PatchReserve([FromRoute] int id)
    {
        DbClanPlayer? caller = await GetCaller().ConfigureAwait(false);
        if (caller is null) return Unauthorized();
        DbClanPlayer? target = await _context.ClanPlayers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
        if (target is null) return NotFound();
        EntityEntry<DbDoc> entity = await _context.Documents.AddAsync(new DbDoc
        {
            Author = caller,
            AuthorId = caller.Id,
            CreationDate = DateTime.UtcNow,
            Info = new DecisionCouncilPlayer
            {
                PlayerId = id,
                Action = target.OnReserve
                    ? DecisionCouncilPlayer.PlayerAction.FromReserve
                    : DecisionCouncilPlayer.PlayerAction.ToReserve
            }.GenerateVotes(_context),
            Body = new()
            {
                Body = target.OnReserve
                    ? "# Заявка о восстановлении из резерва"
                    : "# Заявка о переводе в резерв"
            }
        }.GenerateTitleFromBody()).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(entity.Entity.Id);
    }

    [Authorize(Roles = "advisor")]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> Kick([FromRoute] int id, [FromQuery] PlayerLeaveReason leaveReason)
    {
        DbClanPlayer? caller = await GetCaller().ConfigureAwait(false);
        if (caller is null) return Unauthorized();
        if (!await _context.Players.AnyAsync(x => x.Id == id).ConfigureAwait(false)) return NotFound();
        EntityEntry<DbDoc> entity = await _context.Documents.AddAsync(new DbDoc
        {
            Author = caller,
            AuthorId = caller.Id,
            CreationDate = DateTime.UtcNow,
            Info = new DecisionCouncilPlayerKick
            {
                PlayerId = id,
                Action = DecisionCouncilPlayer.PlayerAction.Generic,
                PlayerLeaveReason = leaveReason,
                SubstitutePlayerId = (await _context.ClanPlayers.FirstAsync(x => x.Rank == PlayerRank.FirstAdvisor)
                    .ConfigureAwait(false)).Id
            }.GenerateVotes(_context),
            Body = new()
            {
                Body = "# Решение о исключении"
            }
        }.GenerateTitleFromBody()).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(entity.Entity.Id);
    }

    private async Task<DbClanPlayer?> GetCaller()
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return null;
        DbClanPlayer? player = await _context.ClanPlayers.FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false);
        return player;
    }
}