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
    private int _newCommanderId;
    private int _prolongDays;

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

    public bool IsAdvisor { get; set; }
    public Player?[] Members { get; set; } = Array.Empty<Player>();

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

    public int? LeaveJoinDocId { get; set; }

    public int ProlongDays
    {
        get => _prolongDays;
        set
        {
            _prolongDays = value;
            MainLayout.Update();
        }
    }

    public int? ProlongDocId { get; set; }
    public int? NewCommanderDocId { get; set; }

    public int NewCommanderId
    {
        get => _newCommanderId;
        set
        {
            _newCommanderId = value;
            MainLayout.Update();
        }
    }

    public int? CloseDocId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await GetMeId();
        Expedition = await HttpClient.GetFromJsonAsync<Expedition>($"api/Expedition/{Id}")
                     ?? throw new ArgumentNullException();
        if (Expedition.Players is null) throw new ArgumentNullException();
        Members = await Task.WhenAll(Expedition.Players.Select(x =>
            HttpClient.GetFromJsonAsync<Player>($"api/Player/{x}")));
        if (Members is null || Members.Any(x => x is null)) throw new ArgumentNullException();
        Members = Members
            .OrderByDescending(x => x!.Id == Expedition.CommanderId)
            .ThenByDescending(x => (x as ClanPlayer)?.Rank)
            .ThenByDescending(x => (x as ClanPlayer)?.Score ?? 0d)
            .ThenBy(x => x!.Nickname)
            .ToArray();
        NewCommanderId = Expedition.CommanderId;
        await SetDocPage(1);
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
        Docs = await response.Content.ReadFromJsonAsync<Doc[]>() ?? throw new InvalidOperationException();
        await PlayerNicknamesService.LoadNicknames(Docs.Select(x => x.AuthorId));
        StateHasChanged();
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

    public async Task CreateJoin()
    {
        using HttpResponseMessage result = await HttpClient.PatchAsync(
            $"api/Expedition/{Id}/join",
            new ByteArrayContent(Array.Empty<byte>()));
        LeaveJoinDocId = await result.Content.ReadFromJsonAsync<int>();
        StateHasChanged();
        MainLayout.Update();
    }

    public async Task CreateLeave()
    {
        using HttpResponseMessage result = await HttpClient.PatchAsync(
            $"api/Expedition/{Id}/leave",
            new ByteArrayContent(Array.Empty<byte>()));
        LeaveJoinDocId = await result.Content.ReadFromJsonAsync<int>();
        StateHasChanged();
        MainLayout.Update();
    }

    public async Task CreateProlong()
    {
        using HttpResponseMessage result = await HttpClient.PatchAsync(
            $"api/Expedition/{Id}/prolong?days={ProlongDays:D}",
            new ByteArrayContent(Array.Empty<byte>()));
        ProlongDocId = await result.Content.ReadFromJsonAsync<int>();
        ProlongDays = 0;
        StateHasChanged();
        MainLayout.Update();
    }

    public async Task CreateTransfer()
    {
        using HttpResponseMessage result = await HttpClient.PatchAsync(
            $"api/Expedition/{Id}/transfer?newCommanderId={NewCommanderId}",
            new ByteArrayContent(Array.Empty<byte>()));
        NewCommanderDocId = await result.Content.ReadFromJsonAsync<int>();
        StateHasChanged();
        MainLayout.Update();
    }

    public async Task CreateClose()
    {
        using HttpResponseMessage result = await HttpClient.PatchAsync(
            $"api/Expedition/{Id}/close",
            new ByteArrayContent(Array.Empty<byte>()));
        CloseDocId = await result.Content.ReadFromJsonAsync<int>();
        StateHasChanged();
        MainLayout.Update();
    }
}