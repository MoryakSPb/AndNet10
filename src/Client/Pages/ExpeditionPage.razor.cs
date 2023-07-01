using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client.Pages;

[Authorize(Roles = "member")]
public partial class ExpeditionPage : ComponentBase
{
    public const int DOCS_ITEMS_ON_PAGE = 10;
    private readonly Dictionary<int, Player> _players = new();

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    public bool IsAdvisor { get; set; }
    public Player[] Members { get; set; } = Array.Empty<Player>();

    [Parameter]
    public Expedition Expedition { get; set; }

    [Parameter]
    public int Id { get; set; }

    public int? MeId { get; set; }

    private Doc[]? Docs { get; set; }
    public int DocsTotalItemsCount { get; set; }
    public bool DocsPreviousPageAvailable => DocsPage > 1;
    public int DocsPage { get; set; } = 1;
    public bool DocsNextPageAvailable => DocsPage < DocsPagesCount;
    private int DocsPagesCount => Math.DivRem(DocsTotalItemsCount, DOCS_ITEMS_ON_PAGE, out int rem) + (rem > 0 ? 1 : 0);

    private void ToPlayerPage(int id)
    {
        NavigationManager.NavigateTo($"/player/{id}");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await GetMeId();
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

    private async Task GetDocs()
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(
            $"api/Expedition/{Id}/docs?skip={DOCS_ITEMS_ON_PAGE * (DocsPage - 1)}&take={DOCS_ITEMS_ON_PAGE}");
        string? itemsCountString = response.Headers.FirstOrDefault(x =>
            string.Equals(x.Key, "Items-Count", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
        if (int.TryParse(itemsCountString, out int itemsCount)) DocsTotalItemsCount = itemsCount;
        Docs = await response.Content.ReadFromJsonAsync<Doc[]>();
        await LoadDocAuthors();
        StateHasChanged();
    }

    private async Task LoadDocAuthors()
    {
        _players.Clear();
        foreach (int playerId in Docs!.Select(x => x.AuthorId).Distinct())
        {
            if (_players.ContainsKey(playerId)) continue;
            Player? player = await HttpClient.GetFromJsonAsync<Player>($"api/Player/{playerId}");
            _players.Add(playerId, player);
        }
    }

    public async Task SetDocPage(int page)
    {
        DocsPage = page;
        await GetDocs();
        StateHasChanged();
    }

    protected async Task GetMeId()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            if (int.TryParse(authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                    out int playerId))
                MeId = playerId;
            else MeId = null;
        }
        else
        {
            MeId = null;
        }

        IsAdvisor = authState.User.IsInRole("advisor");
    }
}