using System.Collections.Immutable;
using System.IO.Hashing;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "member")]
[ApiController]
public class AwardController : ControllerBase
{
    private readonly DatabaseContext _context;

    public AwardController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<Award>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Award[]>> GetAwards([FromQuery] int? playerId = null)
    {
        IQueryable<DbAward> set = _context.Awards.OrderByDescending(x => x.AwardType).ThenByDescending(x => x.IssueDate)
            .AsNoTracking();
        if (playerId is not null) set = set.Where(x => x.PlayerId == playerId);
        HttpContext.Response.Headers["Items-Count"] = (await set.CountAsync().ConfigureAwait(false)).ToString("D");

        DbAward[] results = await set.ToArrayAsync().ConfigureAwait(false);
        string eTag =
            $"W/\"{BitConverter.ToUInt64(XxHash64.Hash(results.SelectMany(x => BitConverter.GetBytes(x.Version)).ToImmutableArray().AsSpan()))}\"";
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = eTag;
        return Ok(results.Select(x => (Award)x).ToArray());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Award), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Award>> GetAward([FromRoute] int id, [FromQuery] bool includeDeleted = false)
    {
        DbAward? dbAward = await (includeDeleted ? _context.Awards.IgnoreQueryFilters() : _context.Awards)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (dbAward is null) return NotFound();
        string eTag = $"W/\"{dbAward.Version:D10}\"";
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = eTag;
        return Ok((Award)dbAward);
    }
}