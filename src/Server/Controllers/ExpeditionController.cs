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
public class ExpeditionController : ControllerBase
{
    private readonly DatabaseContext _context;

    public ExpeditionController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<Expedition>), StatusCodes.Status200OK)]
    public async IAsyncEnumerable<Expedition> GetExpeditions([FromQuery] int skip = 0,
        [FromQuery] int take = int.MaxValue,
        [FromQuery] bool getDeleted = false)
    {
        IQueryable<DbExpedition> set =
            (getDeleted
                ? _context.Expeditions.IgnoreQueryFilters()
                : _context.Expeditions).Include(x => x.Members).AsNoTracking();
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
}