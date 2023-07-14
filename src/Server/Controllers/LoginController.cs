using System.Security.Claims;
using AndNet.Integration.Steam;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace AndNet.Manager.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class LoginController : ControllerBase
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<LoginController> _logger;
    private readonly SignInManager<DbUser> _signInManager;
    private readonly SteamClient _steamClient;
    private readonly UserManager<DbUser> _userManager;

    public LoginController(SignInManager<DbUser> signInManager, ILogger<LoginController> logger,
        DatabaseContext databaseContext, UserManager<DbUser> userManager, SteamClient steamClient)
    {
        _signInManager = signInManager;
        _logger = logger;
        _databaseContext = databaseContext;
        _userManager = userManager;
        _steamClient = steamClient;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult LoginExternal([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        string? actionName = provider switch
        {
            "Steam" => nameof(CallbackSteam),
            "Discord" => nameof(CallbackDiscord),
            _ => null
        };
        if (actionName is null) return BadRequest();
        string? redirectUrl = Url.Action(actionName, new
        {
            returnUrl = actionName == nameof(CallbackDiscord)
                ? "https://andnet.eclipse-interactive.xyz/api/auth/login-callback/discord"
                : returnUrl
        });
        AuthenticationProperties properties =
            _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet("claims")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ClaimRecord[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IAsyncEnumerable<ClaimRecord>>> Claims()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return Unauthorized();
        DbUser? user = await _userManager.GetUserAsync(HttpContext.User).ConfigureAwait(false);
        if (user is null) return NotFound();
        int? playerId = (await _databaseContext.ClanPlayers.Select(x => new { x.Id, x.IdentityId })
            .FirstOrDefaultAsync(x => x.IdentityId == user.Id)
            .ConfigureAwait(false))?.Id;
        if (playerId is null) return NotFound();
        return Ok(User.Claims
            .Where(x => x.Type != "AspNet.Identity.SecurityStamp")
            .Select(x =>
            {
                string value = (x.Type == ClaimTypes.NameIdentifier ? playerId.ToString() : x.Value)!;
                return new ClaimRecord(x.Type, value, x.Issuer, x.OriginalIssuer, x.ValueType);
            })
            .ToAsyncEnumerable());
    }

    [HttpHead("claims")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ResponseCache(NoStore = true)]
    public IActionResult ClaimsHead()
    {
        return User.Identity?.IsAuthenticated ?? false ? Ok() : Unauthorized();
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(User.Identity?.AuthenticationType).ConfigureAwait(false);
        return LocalRedirect("/");
    }

    [HttpGet("login-callback/discord")]
    [AllowAnonymous]
    public async Task<IActionResult> CallbackDiscord([FromQuery] string? returnUrl = null,
        [FromQuery] string? code = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!Url.IsLocalUrl(returnUrl)) returnUrl = Url.Content("~/");
        ExternalLoginInfo? info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
        if (info == null) return BadRequest();

        SignInResult result = await _signInManager
            .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, false).ConfigureAwait(false);

        if (result.IsLockedOut || result.IsNotAllowed)
            return Forbid();
        if (result.RequiresTwoFactor) return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl });
        if (result.Succeeded)
        {
            await HttpContext.SignInAsync(info.Principal)
                .ConfigureAwait(false);
            return LocalRedirect(returnUrl);
        }

        if (!ulong.TryParse(info.ProviderKey, out ulong discordId)) return BadRequest();
        DbClanPlayer? player = await _databaseContext.ClanPlayers.Include(x => x.Identity)
            .FirstOrDefaultAsync(x => x.DiscordId == discordId).ConfigureAwait(false);

        if (player is null) return NotFound();
        if (player.Identity is null)
        {
            DbUser newUser = new()
            {
                Player = player,
                UserName = player.Nickname,
                ConcurrencyStamp = Guid.NewGuid().ToString("n"),
                SecurityStamp = Guid.NewGuid().ToString("n")
            };
            await _userManager.CreateAsync(newUser).ConfigureAwait(false);
            //player.IdentityId = newUser.Id;
            player.Identity = newUser;
        }

        await _userManager.AddLoginAsync(player.Identity,
            new ExternalLoginInfo(info.Principal, info.LoginProvider, info.ProviderKey,
                info.ProviderDisplayName ?? string.Empty)).ConfigureAwait(false);
        await _userManager.SetLockoutEnabledAsync(player.Identity, false).ConfigureAwait(false);
        List<string> roles = new() { "member" };
        if (player.Rank >= PlayerRank.Advisor) roles.Add("advisor");
        if (player.Rank == PlayerRank.FirstAdvisor) roles.Add("first_advisor");
        await _userManager.AddToRolesAsync(player.Identity, roles).ConfigureAwait(false);
        await _userManager.UpdateAsync(player.Identity).ConfigureAwait(false);
        return await CallbackDiscord(returnUrl, code).ConfigureAwait(false);
    }

    [HttpGet("login-callback/steam")]
    [AllowAnonymous]
    public async Task<IActionResult> CallbackSteam([FromQuery] string? returnUrl = null,
        [FromQuery] string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError is not null)
            //ErrorMessage = $"Error from external provider: {remoteError}";
            return BadRequest(remoteError);
        ExternalLoginInfo? info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
        if (info == null) return BadRequest();

        SignInResult result = await _signInManager
            .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, false).ConfigureAwait(false);

        if (result.IsLockedOut || result.IsNotAllowed)
            return Forbid();
        if (result.RequiresTwoFactor) return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl });
        if (result.Succeeded)
        {
            await HttpContext.SignInAsync(info.Principal)
                .ConfigureAwait(false);
            return LocalRedirect(returnUrl);
        }


        ulong? steamId = await _steamClient.ResolveSteamUrlAsync(info.ProviderKey).ConfigureAwait(false);
        DbClanPlayer? player = await _databaseContext.ClanPlayers.Include(x => x.Identity)
            .FirstOrDefaultAsync(x => x.SteamId == steamId).ConfigureAwait(false);

        if (player is null) return NotFound();
        if (player.Identity is null)
        {
            DbUser newUser = new()
            {
                Player = player,
                UserName = player.Nickname,
                ConcurrencyStamp = Guid.NewGuid().ToString("n"),
                SecurityStamp = Guid.NewGuid().ToString("n")
            };
            await _userManager.CreateAsync(newUser).ConfigureAwait(false);
            //player.IdentityId = newUser.Id;
            player.Identity = newUser;
        }

        await _userManager.AddLoginAsync(player.Identity,
            new ExternalLoginInfo(info.Principal, info.LoginProvider, info.ProviderKey,
                info.ProviderDisplayName ?? string.Empty)).ConfigureAwait(false);
        await _userManager.SetLockoutEnabledAsync(player.Identity, false).ConfigureAwait(false);
        List<string> roles = new() { "member" };
        if (player.Rank >= PlayerRank.Advisor) roles.Add("advisor");
        if (player.Rank == PlayerRank.FirstAdvisor) roles.Add("first_advisor");
        await _userManager.AddToRolesAsync(player.Identity, roles).ConfigureAwait(false);
        await _userManager.UpdateAsync(player.Identity).ConfigureAwait(false);
        return await CallbackSteam(returnUrl, remoteError).ConfigureAwait(false);
    }

    [HttpPost("2fa")]
    public async Task<IActionResult> Post(
        [FromBody] string code,
        [FromQuery] bool isRecovery = false,
        [FromQuery] bool rememberMachine = false,
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        IdentityUser<int>? user = await _signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(false);
        if (user is null) return NotFound();

        SignInResult result = !isRecovery
            ? await _signInManager
                .TwoFactorRecoveryCodeSignInAsync(code.Replace(" ", string.Empty).Replace("-", string.Empty))
                .ConfigureAwait(false)
            : await _signInManager
                .TwoFactorAuthenticatorSignInAsync(code.Replace(" ", string.Empty).Replace("-", string.Empty),
                    rememberMachine, rememberMachine).ConfigureAwait(false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }

        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Forbid();
    }
}