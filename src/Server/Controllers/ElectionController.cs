using System.Collections.Immutable;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using AndNet.Manager.Shared.Models.Election;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "member")]
[ApiController]
public class ElectionController : Controller
{
    private readonly DatabaseContext _context;
    private readonly DocumentService _documentService;
    private readonly UserManager<DbUser> _userManager;

    public ElectionController(DatabaseContext context, UserManager<DbUser> userManager, DocumentService documentService)
    {
        _context = context;
        _userManager = userManager;
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<Election> GetCurrent()
    {
        DbElection result = await _context.Elections
                                .Include(x => x.ElectionCandidates)
                                .Include(x => x.Voters)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Stage != ElectionStage.Ended).ConfigureAwait(false)
                            ?? throw new InvalidOperationException();
        IImmutableList<ElectionCandidate> candidates = (result.Stage < ElectionStage.ResultsAnnounce
                ? result.Candidates.Select(x => x with { Rating = 0 })
                : result.Candidates)
            .OrderByDescending(x => x.Rating).ThenBy(x => x.RegistrationDate)
            .ToImmutableList();

        return new()
        {
            AllVotersCount = result.AllVotersCount,
            Candidates = candidates,
            CouncilCapacity = result.CouncilCapacity,
            ElectionEnd = result.ElectionEnd,
            Id = result.Id,
            Stage = result.Stage,
            VotedVotersCount = result.VotedVotersCount
        };
    }

    [HttpGet("isVoted")]
    public async Task<ActionResult<bool>> GetIsVoted()
    {
        var election = await _context.Elections
                           .Select(x => new { x.Id, x.Stage })
                           .FirstOrDefaultAsync(x => x.Stage != ElectionStage.Ended).ConfigureAwait(false)
                       ?? throw new InvalidOperationException();
        if (election.Stage != ElectionStage.Voting) return StatusCode(StatusCodes.Status412PreconditionFailed);

        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return Forbid();
        int playerId = await _context.ClanPlayers
            .Where(x => x.IdentityId == user.Id)
            .Take(1).Select(x => x.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (playerId == default) return Forbid();

        DbElectionVoter? voter = _context.ElectionsVoters.AsNoTracking()
            .FirstOrDefault(x => x.ElectionId == election.Id && x.PlayerId == playerId);
        if (voter is null) return Forbid();
        return Ok(voter.VoteDate is not null);
    }

    [HttpGet("all")]
    public async IAsyncEnumerable<Election> GetAll()
    {
        await foreach (DbElection election in _context.Elections
                           .Include(x => x.ElectionCandidates)
                           .Include(x => x.Voters)
                           .AsNoTracking()
                           .OrderByDescending(x => x.ElectionEnd)
                           .ToAsyncEnumerable().ConfigureAwait(false))
        {
            IImmutableList<ElectionCandidate> candidates = election.Stage < ElectionStage.ResultsAnnounce
                ? election.Candidates.Select(x => x with { Rating = 0 }).ToImmutableList()
                : election.Candidates;
            yield return new()
            {
                AllVotersCount = election.AllVotersCount,
                Candidates = candidates,
                CouncilCapacity = election.CouncilCapacity,
                ElectionEnd = election.ElectionEnd,
                Id = election.Id,
                Stage = election.Stage,
                VotedVotersCount = election.VotedVotersCount
            };
        }
    }

    [HttpPatch]
    public async Task<IActionResult> Reg()
    {
        var election = await _context.Elections
                           .Select(x => new { x.Id, x.Stage })
                           .FirstOrDefaultAsync(x => x.Stage != ElectionStage.Ended).ConfigureAwait(false)
                       ?? throw new InvalidOperationException();
        if (election.Stage != ElectionStage.Registration) return StatusCode(StatusCodes.Status412PreconditionFailed);

        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return Forbid();
        int playerId = await _context.ClanPlayers
            .Where(x => x.IdentityId == user.Id
                        && !(x.OnReserve
                             || x.Rank < PlayerRank.MiddleSpecialist
                             || x.Rank == PlayerRank.FirstAdvisor))
            .Take(1).Select(x => x.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (playerId == default) return Forbid();


        DbElectionCandidate? reg = await _context.ElectionsCandidates
            .FirstOrDefaultAsync(x => x.ElectionId == election.Id && x.PlayerId == playerId)
            .ConfigureAwait(false);
        if (reg is null)
            await _context.ElectionsCandidates.AddAsync(new()
            {
                PlayerId = playerId,
                ElectionId = election.Id,
                Rating = 0,
                IsWinner = false,
                RegistrationDate = DateTime.UtcNow
            }).ConfigureAwait(false);
        else
            _context.ElectionsCandidates.Remove(reg);

        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Vote([FromBody] IDictionary<int, bool> bulletin)
    {
        var election = await _context.Elections
                           .Select(x => new { x.Id, x.Stage })
                           .FirstOrDefaultAsync(x => x.Stage != ElectionStage.Ended).ConfigureAwait(false)
                       ?? throw new InvalidOperationException();
        if (election.Stage != ElectionStage.Voting) return StatusCode(StatusCodes.Status412PreconditionFailed);

        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return Forbid();
        var player = await _context.ClanPlayers
            .Where(x => x.IdentityId == user.Id)
            .Take(1).Select(x => new { x.Id, x.Rank })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        if (player == default) return Forbid();

        DbElectionVoter? voter =
            _context.ElectionsVoters.FirstOrDefault(x => x.ElectionId == election.Id && x.PlayerId == player.Id);
        if (voter is null) return Forbid();
        if (voter.VoteDate is not null) return Conflict();

        foreach (DbElectionCandidate candidate in _context.ElectionsCandidates.Where(x => x.ElectionId == election.Id))
        {
            if (!bulletin.TryGetValue(candidate.PlayerId, out bool result)) return BadRequest();
            if (result) candidate.Rating++;
            else candidate.Rating--;
        }

        voter.VoteDate = DateTime.UtcNow;
        DbClanPlayer? firstAdvisor = null;
        DbDoc? doc = null;
        if (player.Rank < PlayerRank.Advisor)
        {
            firstAdvisor = await _context.ClanPlayers
                .AsNoTrackingWithIdentityResolution()
                .FirstAsync(x => x.Rank == PlayerRank.FirstAdvisor).ConfigureAwait(false);
            doc = new()
            {
                Author = firstAdvisor,
                Id = firstAdvisor.Id,
                AuthorId = firstAdvisor.Id,
                CreationDate = DateTime.UtcNow,
                Info = new DecisionCouncilPlayerAwardSheet
                {
                    PlayerId = player.Id,
                    Votes = ImmutableList<Decision.Vote>.Empty,
                    MinYesVotesPercent = AwardRules.MinCouncilVotes[AwardType.Copper],
                    Action = DecisionCouncilPlayer.PlayerAction.Generic,
                    AwardType = AwardType.Copper,
                    AutomationId = 3,
                    Description = "Участие в выборах совета в качестве избирателя",
                    PredeterminedIssueDate = voter.VoteDate
                },
                Body = new()
                {
                    Body = @"# Наградной лист

Награда выдана автоматически."
                }
            };
            await _context.Documents.AddAsync(doc).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
        if (doc is not null) await _documentService.AgreeExecuteAsync(doc, firstAdvisor!).ConfigureAwait(false);
        return Ok();
    }
}