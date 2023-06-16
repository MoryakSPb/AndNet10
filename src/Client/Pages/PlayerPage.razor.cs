using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

public partial class PlayerPage : ComponentBase
{
    private readonly Dictionary<int, Player> _players = new();
    private Award[]? Awards;

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    [Parameter]
    public Player? Model { get; set; }

    [Parameter]
    public int Id { get; set; }

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    private IEnumerable<(AwardType Key, int)> GetCountsByType
    {
        get
        {
            Dictionary<AwardType, int> values =
                Awards!.GroupBy(x => x.AwardType).ToDictionary(x => x.Key, x => x.Count());
            return Enum.GetValues<AwardType>().OrderByDescending(x => (int)x)
                .Select(x => (x, values.GetValueOrDefault(x, 0)));
        }
    }

    private async Task LoadAwardIssuers()
    {
        _players.Clear();
        foreach (int playerId in Awards!.Where(x => x.IssuerId.HasValue).Select(x => x.IssuerId!.Value).Distinct())
        {
            Player? player = await HttpClient.GetFromJsonAsync<Player>($"api/Player/{playerId}");
            _players.Add(playerId, player);
        }
    }

    private async Task GetAwards()
    {
        if (Awards is not null) return;
        Awards = await HttpClient.GetFromJsonAsync<Award[]>($"api/award?playerId={Id}");
        await LoadAwardIssuers();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Model = await HttpClient.GetFromJsonAsync<Player>($"api/Player/{Id}");
        StateHasChanged();
        MainLayout.Update();
        await GetAwards();
    }
}