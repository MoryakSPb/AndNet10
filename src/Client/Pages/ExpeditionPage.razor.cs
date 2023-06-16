using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

[Authorize(Roles = "member")]
public partial class ExpeditionPage : ComponentBase
{
    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    public Player[] Members { get; set; } = Array.Empty<Player>();

    [Parameter]
    public Expedition Expedition { get; set; }

    [Parameter]
    public int Id { get; set; }

    private void ToPlayerPage(int id)
    {
        NavigationManager.NavigateTo($"/player/{id}");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Expedition = await HttpClient.GetFromJsonAsync<Expedition>($"api/Expedition/{Id}")
                     ?? throw new ArgumentNullException();
        if (Expedition.Players is null) throw new ArgumentNullException();
        Members = await Task.WhenAll(Expedition.Players.Select(x =>
            HttpClient.GetFromJsonAsync<Player>($"api/Player/{x}")));
        Members = Members
            .OrderByDescending(x => x.Id == Expedition.CommanderId)
            .ThenByDescending(x => (x as ClanPlayer)?.Rank)
            .ThenByDescending(x => (x as ClanPlayer)?.Score ?? 0d)
            .ThenBy(x => x.Nickname)
            .ToArray();
        StateHasChanged();
        MainLayout.Update();
    }
}