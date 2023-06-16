using System.Net.Http.Json;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

[Authorize(Roles = "member")]
public partial class Expeditions : ComponentBase
{
    private readonly Dictionary<int, Player> _players = new();

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Expedition[]? Models { get; set; }

    [Parameter]
    public bool ShowInactive { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Models = await HttpClient.GetFromJsonAsync<Expedition[]>("api/Expedition?getDeleted=true");
        _players.Clear();
        foreach (Player? player in await Task.WhenAll(Models!.Select(x =>
                     HttpClient.GetFromJsonAsync<Player>($"api/Player/{x.CommanderId}"))))
            if (player is not null)
                _players.Add(player.Id, player);
        StateHasChanged();
    }

    private void ToExpeditionPage(int id)
    {
        NavigationManager.NavigateTo($"/expedition/{id}");
    }
}