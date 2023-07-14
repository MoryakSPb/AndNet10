using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "member")]
public class DecisionController : Controller
{
    private readonly DatabaseContext _context;
    private readonly DocumentService _documentService;
    private readonly UserManager<DbUser> _userManager;

    public DecisionController(DatabaseContext context, UserManager<DbUser> userManager, DocumentService documentService)
    {
        _context = context;
        _userManager = userManager;
        _documentService = documentService;
    }

    [HttpHead("{id:int}")]
    [ResponseCache(Duration = 60000, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> ExecuteSupported([FromRoute] int id)
    {
        IQueryable<DbDoc> set = _context.Documents
            .IgnoreQueryFilters()
            .AsNoTracking();
        Doc? doc = await set.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (doc is null) return NotFound();
        return doc.Info is not null && DocumentService.ExecuteSupported(doc.Info.GetType())
            ? Ok(doc)
            : NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Vote([FromRoute] int id, [FromQuery(Name = "vote")] VoteType voteType)
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return Forbid();
        int playerId = await _context.ClanPlayers.Where(x => x.IdentityId == user.Id).Take(1).Select(x => x.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (playerId == default) return Forbid();

        DbDoc? doc = await _context.Documents
            .FirstOrDefaultAsync(x => x.Id == id
                                      && x.Info!.Type.StartsWith("Р")
                                      && EF.Functions.JsonContains(x.Info, @"{""IsExecuted"": null}"))
            .ConfigureAwait(false);
        if (doc?.Info is not Decision decision) return NotFound();
        Decision.Vote? vote = decision.Votes.FirstOrDefault(x => x.PlayerId == playerId);
        if (vote is null) return Forbid();
        vote.Date = DateTime.UtcNow;
        vote.VoteType = voteType;
        _context.Update(doc).State = EntityState.Modified;
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    [HttpGet("{id:int}/execute")]
    [Authorize(Roles = "advisor")]
    public async Task<IActionResult> Execute([FromRoute] int id, [FromQuery(Name = "agree")] bool isAgree)
    {
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return Forbid();
        DbClanPlayer? player = await _context.ClanPlayers.FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false);
        if (player is null) return Forbid();

        DbDoc? doc = await _context.Documents
            .FirstOrDefaultAsync(x => x.Id == id
                                      && x.Info!.Type.StartsWith("Р")
                                      && EF.Functions.JsonContains(x.Info, @"{""IsExecuted"": null}"))
            .ConfigureAwait(false);
        if (doc is null) return NotFound();

        try
        {
            if (isAgree)
                await _documentService.AgreeExecuteAsync(doc, player).ConfigureAwait(false);
            else
                _documentService.DeclineExecuteAsync(doc, player);
        }
        catch (InvalidOperationException)
        {
            return BadRequest();
        }

        _context.Update(doc).State = EntityState.Modified;
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }
}