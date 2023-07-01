using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client.Shared;

public partial class NavMenu
{
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    public string? UserName { get; set; }
    public int? PlayerId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            UserName = authState.User.Identity?.Name;
            if (int.TryParse(authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                    out int playerId))
                PlayerId = playerId;
            else PlayerId = null;
        }
        else
        {
            UserName = null;
            PlayerId = null;
        }
    }
}