using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Client.Shared.Player;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client.Shared.Expedition;

public partial class ExpeditionCreateDialog : ComponentBase
{
    public int Days { get; set; } = 14;

    [Parameter]
    public string Description { get; set; } = string.Empty;

    public int MeId { get; set; }
    public PlayerSelect PlayerSelect { get; set; }

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

    public int? DocumentId { get; set; } = null;
    public List<int> AllPlayers { get; set; } = null!;
    public int[] LockedPlayers { get; set; } = null!;
    public List<int> SelectedPlayers { get; set; } = null!;

    [Parameter]
    public int SelectedPlayerCount { get; set; }

    private Manager.Shared.Models.Player[] Players { get; set; } = null!;

    protected async Task GetMeId()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            if (int.TryParse(authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                    out int playerId))
                MeId = playerId;
            else throw new InvalidOperationException();
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetMeId();
        Players = await HttpClient.GetFromJsonAsync<Manager.Shared.Models.Player[]>("api/Player")
                  ?? throw new InvalidOperationException();
        foreach (Manager.Shared.Models.Player player in Players)
            PlayerNicknamesService.Cache =
                PlayerNicknamesService.Cache.SetItem(player.Id, (DateTime.UtcNow, player.FullNickname));
        AllPlayers = Players.Select(x => x.Id).Where(x => x != MeId).ToList();
        LockedPlayers = new[] { MeId };
        SelectedPlayers = LockedPlayers.ToList();
        await base.OnInitializedAsync();
    }

    public async Task Send()
    {
        using HttpResponseMessage result = await HttpClient.PostAsJsonAsync("api/expedition",
            new ExpeditionCreateRequest
            {
                Days = Days,
                Description = Description,
                Members = PlayerSelect.SelectedPlayers.ToImmutableArray()
            });
        if (result.IsSuccessStatusCode) DocumentId = await result.Content.ReadFromJsonAsync<int>();
        StateHasChanged();
    }
}