using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using static AndNet.Manager.Shared.Models.Documentation.Info.DirectiveInfo;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "member")]
[ApiController]
public class DocumentController : Controller
{
    private readonly DatabaseContext _context;
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly UserManager<DbUser> _userManager;

    public DocumentController(DatabaseContext context, UserManager<DbUser> userManager)
    {
        _context = context;
        _userManager = userManager;
        MarkdownPipelineBuilder pipelineBuilder = new();
        pipelineBuilder = pipelineBuilder
            .UseAdvancedExtensions()
            .UseBootstrap();
        _markdownPipeline = pipelineBuilder.Build();
    }

    [HttpGet]
    public async IAsyncEnumerable<Doc> GetDocs([FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] DocumentCategory category = DocumentCategory.All,
        [FromQuery] DocumentSortType sortType = DocumentSortType.NoSort,
        [FromQuery] int? decisionStatus = null,
        [FromQuery] DirectiveStatus? directiveStatus = null,
        [FromQuery] int? playerId = null,
        [FromQuery] int? expeditionId = null,
        [FromQuery] long start = 0L,
        [FromQuery] long end = 4102444800L,
        [FromQuery] string? searchString = null)
    {
        if (category == default) category = DocumentCategory.All;
        IQueryable<DbDoc> set = _context.Documents.AsNoTracking();
        set = set.Where(x =>
            x.CreationDate >= DateTime.UnixEpoch.AddSeconds(start)
            && x.CreationDate <= DateTime.UnixEpoch.AddSeconds(end));
        switch (category)
        {
            case DocumentCategory.Directive:
                set = set.Where(x => x.Info!.Type.StartsWith("Д"));
                set = directiveStatus switch
                {
                    DirectiveStatus.Replaced => set.Where(x =>
                        !EF.Functions.JsonContains(x.Info!, "{\"ReplacedById\": null}")),
                    DirectiveStatus.Canceled => set.Where(x =>
                        EF.Functions.JsonContains(x.Info!, "{\"ReplacedById\": null}")
                        && !EF.Functions.JsonContains(x.Info!, "{\"CancelDate\": null}")),
                    DirectiveStatus.Accepted => set.Where(x =>
                        EF.Functions.JsonContains(x.Info!, "{\"ReplacedById\": null, \"CancelDate\": null}")
                        && !EF.Functions.JsonContains(x.Info!, "{\"AcceptanceDate\": null}")),
                    DirectiveStatus.Project => set.Where(x => EF.Functions.JsonContains(x.Info!,
                        "{\"ReplacedById\": null, \"CancelDate\": null, \"AcceptanceDate\": null}")),
                    null => set,
                    _ => throw new ArgumentOutOfRangeException(nameof(directiveStatus), directiveStatus, null)
                };
                break;
            case DocumentCategory.Report:
                set = set.Where(x => x.Info!.Type.StartsWith("О"));
                break;
            case DocumentCategory.Protocol:
                set = set.Where(x => x.Info!.Type.StartsWith("П"));
                break;
            case DocumentCategory.Decision:
                set = set.Where(x => x.Info!.Type.StartsWith("Р"));
                set = decisionStatus switch
                {
                    0 => set.Where(x => EF.Functions.JsonContains(x.Info!, "{\"IsExecuted\": null}")),
                    -1 => set.Where(x => EF.Functions.JsonContains(x.Info!, "{\"IsExecuted\": false}")),
                    +1 => set.Where(x => EF.Functions.JsonContains(x.Info!, "{\"IsExecuted\": true}")),
                    _ => set
                };
                break;
            case DocumentCategory.All:
            default:
                break;
        }

        if (playerId is not null)
            set = set.Where(x => EF.Functions.JsonContains(x.Info!, $"{{\"PlayerId\": {playerId.Value} }}"));
        if (expeditionId is not null)
            set = set.Where(x => EF.Functions.JsonContains(x.Info!, $"{{\"ExpeditionId\": {expeditionId.Value} }}"));
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            IQueryable<int> result;
            if (!searchString.Any(char.IsWhiteSpace))
            {
                searchString += ":*";
                result = _context.DocumentBodies
                    .Where(x => x.SearchVector.Matches(EF.Functions.ToTsQuery("russian", searchString)))
                    .OrderBy(x => x.SearchVector.Distance(EF.Functions.ToTsQuery("russian", searchString)))
                    .Select(x => x.DocId);
            }
            else
            {
                result = _context.DocumentBodies
                    .Where(x => x.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("russian", searchString)))
                    .OrderBy(x => x.SearchVector.Distance(EF.Functions.WebSearchToTsQuery("russian", searchString)))
                    .Select(x => x.DocId);
            }

            HttpContext.Response.Headers["Items-Count"] =
                (await result.CountAsync().ConfigureAwait(false)).ToString("D");
            set = result.Join(set, x => x, x => x.Id, (id, doc) => doc);
        }
        else
        {
            HttpContext.Response.Headers["Items-Count"] =
                (await set.CountAsync().ConfigureAwait(false)).ToString("D");
            set = sortType switch
            {
                DocumentSortType.NoSort => set.OrderBy(x => x.Id),
                DocumentSortType.Views => set.OrderByDescending(x => x.Views),
                DocumentSortType.CreationDate => set.OrderByDescending(x => x.CreationDate),
                _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
            };
        }


        take = Math.Min(take, 100);
        foreach (DbDoc doс in set.Skip(skip).Take(take))
            yield return doс;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Doc), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Doc>> GetDoc([FromRoute] int id)
    {
        IQueryable<DbDoc> set = _context.Documents
            .IgnoreQueryFilters()
            .AsNoTracking();
        Doc? doc = await set.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        if (doc is null) return NotFound();
        string eTag = $"W/\"{doc.Version:D10}\"";
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = eTag;
        return Ok(doc);
    }

    [HttpGet("{id:int}/body")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDocBody([FromRoute] int id)
    {
        IQueryable<DbDocBody> set = _context.DocumentBodies
            .IgnoreQueryFilters()
            .Include(x => x.Doc);
        DbDocBody? dbDocBody = await set.FirstOrDefaultAsync(x => x.DocId == id).ConfigureAwait(false);
        if (dbDocBody is null) return NotFound();
        dbDocBody.Doc.Views++;
        await _context.SaveChangesAsync().ConfigureAwait(false);
        string eTag = $"W/\"{dbDocBody.Version:D10}\"";
        if (Request.Headers.IfNoneMatch.ToString() == eTag) return StatusCode(StatusCodes.Status304NotModified);
        Response.Headers.ETag = eTag;
        return Ok(Markdown.ToHtml(dbDocBody.Body, _markdownPipeline));
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCustomDoc([FromBody] DocWithBody doc)
    {
        DbClanPlayer? caller = await GetCaller().ConfigureAwait(false);
        if (caller is null) return Unauthorized();

        if (doc.Info is not null && DocumentService.ExecuteSupported(doc.Info.GetType())) return UnprocessableEntity();
        if (doc.Info is Decision && caller.Rank < PlayerRank.Advisor) return Forbid();

        if (doc.Info is Decision decision)
        {
            decision.IsExecuted = null;
            decision.ExecuteDate = null;
            decision.ExecutorId = null;
        }

        EntityEntry<DbDoc> result = await _context.Documents.AddAsync(new()
        {
            AuthorId = caller.Id,
            Body = new()
            {
                Body = doc.Body
            },
            CreationDate = DateTime.UtcNow,
            Info = doc.Info,
            Title = doc.Title,
            ParentId = doc.ParentId,
            Views = 0
        }).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return Ok(result.Entity.Id);
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