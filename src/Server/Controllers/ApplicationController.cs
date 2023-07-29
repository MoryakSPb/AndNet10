using AndNet.Integration.Discord.Services;
using AndNet.Integration.Steam;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly DiscordService _discordService;
    private readonly ILogger<ApplicationController> _logger;
    private readonly SteamClient _steamClient;

    public ApplicationController(DatabaseContext context, ILogger<ApplicationController> logger,
        SteamClient steamClient, DiscordService discordService)
    {
        _context = context;
        _logger = logger;
        _steamClient = steamClient;
        _discordService = discordService;
    }

    [HttpGet("steam")]
    [AllowAnonymous]
    public async Task<ActionResult<ulong>> GetSteamId([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return NotFound();
        ulong? result = await _steamClient.ResolveSteamUrlAsync(url).ConfigureAwait(false);
        return result.HasValue ? Ok(result.Value) : NotFound();
    }

    [HttpGet("discord")]
    [AllowAnonymous]
    public async Task<ActionResult<ulong>> GetDiscordId([FromQuery] string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return NotFound();
        ulong? result = await _discordService.GetIdFromUserName(username).ConfigureAwait(false);
        return result.HasValue ? Ok(result.Value) : NotFound();
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    public async Task<ActionResult> Post([FromBody] PlayerApplicationRequest applicationRequest)
    {
        applicationRequest.Nickname = applicationRequest.Nickname.Trim();
        if (string.IsNullOrWhiteSpace(applicationRequest.Nickname)) return BadRequest();
        DbPlayer[] conflictPlayers = await _context.Players.AsNoTracking().Where(x =>
                x.SteamId == applicationRequest.SteamId
                || x.DiscordId
                == applicationRequest.DiscordId
                || x.Nickname
                == applicationRequest.Nickname)
            .ToArrayAsync().ConfigureAwait(false);

        DbExternalPlayer player;
        switch (conflictPlayers.Length)
        {
            case > 1:
                return Conflict();
            case 1:
            {
                DbPlayer singlePlayer = conflictPlayers[0];
                switch (singlePlayer)
                {
                    case DbClanPlayer:
                        return NoContent();
                    case DbFormerClanPlayer { RestorationAvailable: false }:
                        return Forbid();
                    case DbExternalPlayer externalPlayer:
                        player = externalPlayer;
                        player.Nickname = applicationRequest.Nickname;
                        player.DiscordId ??= applicationRequest.DiscordId;
                        player.SteamId ??= applicationRequest.SteamId;
                        break;
                    default:
                        throw new("Invalid player type");
                }

                break;
            }
            default:
                player = new()
                {
                    SteamId = applicationRequest.SteamId,
                    Nickname = applicationRequest.Nickname,
                    DiscordId = applicationRequest.DiscordId,
                    RealName = string.IsNullOrWhiteSpace(applicationRequest.RealName)
                        ? null
                        : applicationRequest.RealName,
                    Status = PlayerStatus.External,
                    DetectionDate = DateTime.UtcNow,
                    Relationship = PlayerRelationship.Unknown
                };
                _context.ExternalPlayers.Add(player);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                break;
        }

        TimeZoneInfo? timeZoneInfo;
        try
        {
            timeZoneInfo = applicationRequest.TimeZoneId is null
                ? null
                : TimeZoneInfo.FindSystemTimeZoneById(applicationRequest.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return BadRequest();
        }
        

        DbDoc doc = new()
        {
            AuthorId = player.Id,
            CreationDate = DateTime.UtcNow,
            Body = new DbDocBody()
            {
                Body = "# Заявка на вступление в клан" + Environment.NewLine + applicationRequest.Description,
            },
            Views = 0,
            Info = new DecisionCouncilPlayerAcceptApplication()
            {
                Action = DecisionCouncilPlayer.PlayerAction.Generic,
                IsExecuted = null,
                ExecutorId = null,
                ExecuteDate = null,
                CurrentAge = applicationRequest.Age,
                Hours = applicationRequest.Hours,
                Recommendation = applicationRequest.Recommendation,
                PlayerId = player.Id,
                TimeZone = applicationRequest.TimeZone,
            }.GenerateVotes(_context)
        };
        doc = doc.GenerateTitleFromBody();
        await _context.Documents.AddAsync(doc).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }
}