using System.Net.Http.Json;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared;

public partial class PlayerEditor : ComponentBase
{
    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string NewNickname { get; set; } = string.Empty;

    [Parameter]
    public string NewRealName { get; set; } = string.Empty;

    [Parameter]
    public string NewTimeZoneInfo { get; set; } = string.Empty;

    [Parameter]
    public bool OnReserve { get; set; }

    public async Task SendNewNickname()
    {
        await HttpClient.PatchAsJsonAsync("api/player/me", new PlayerPatch(NewNickname, null, null));
        NavigationManager.NavigateTo(NavigationManager.Uri, true);
        StateHasChanged();
    }

    public async Task SendNewRealName()
    {
        await HttpClient.PatchAsJsonAsync("api/player/me", new PlayerPatch(null, NewRealName, null));
        NavigationManager.NavigateTo(NavigationManager.Uri, true);
        StateHasChanged();
    }

    public async Task SendNewTimeZoneInfo()
    {
        await HttpClient.PatchAsJsonAsync("api/player/me", new PlayerPatch(null, null, NewTimeZoneInfo));
        NavigationManager.NavigateTo(NavigationManager.Uri, true);
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        Manager.Shared.Models.Player player =
            await HttpClient.GetFromJsonAsync<Manager.Shared.Models.Player>("api/Player/me", SerializationContext.Default.Player)
            ?? throw new InvalidOperationException();
        NewNickname = player.Nickname;
        NewRealName = player.RealName ?? string.Empty;
        NewTimeZoneInfo = player.TimeZone ?? string.Empty;
        ;
    }
}