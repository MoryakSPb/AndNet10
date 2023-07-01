using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "member")]
[ApiController]
public class DocumentController : Controller
{
    private readonly DatabaseContext _context;
    private readonly MarkdownPipeline _markdownPipeline;

    public DocumentController(DatabaseContext context)
    {
        _context = context;
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
        [FromQuery] long start = 0L,
        [FromQuery] long end = 4102444800L,
        [FromQuery] string? searchString = null)
    {
        if (category == default) category = DocumentCategory.All;
        IQueryable<DbDoc> set = _context.Documents.AsNoTracking();
        set = set.Where(x =>
            x.CreationDate >= DateTime.UnixEpoch.AddSeconds(start)
            && x.CreationDate <= DateTime.UnixEpoch.AddSeconds(end));
        if (category != DocumentCategory.All)
            set = category switch
            {
                DocumentCategory.Directive => set.Where(x => x.Info!.Type.StartsWith("Д")),
                DocumentCategory.Report => set.Where(x => x.Info!.Type.StartsWith("О")),
                DocumentCategory.Protocol => set.Where(x => x.Info!.Type.StartsWith("П")),
                DocumentCategory.Decision => set.Where(x => x.Info!.Type.StartsWith("Р")),
                _ => set
            };
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
        foreach (DbDoc doc in set.Skip(skip).Take(take))
            yield return doc;
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
        return Ok(doc);
    }

    [HttpGet("{id:int}/body")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 6000, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult> GetDocBody([FromRoute] int id)
    {
        IQueryable<DbDocBody> set = _context.DocumentBodies
            .IgnoreQueryFilters()
            .Include(x => x.Doc);
        DbDocBody? dbDoc = await set.FirstOrDefaultAsync(x => x.DocId == id).ConfigureAwait(false);
        if (dbDoc is null) return NotFound();
        dbDoc.Doc.Views++;
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok(Markdown.ToHtml(dbDoc.Body, _markdownPipeline));
    }
}