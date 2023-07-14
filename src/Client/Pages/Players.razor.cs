using System.Net.Http.Json;
using System.Text.Encodings.Web;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AndNet.Manager.Client.Pages;

[Authorize(Roles = "member")]
public partial class Players : ComponentBase
{
    public const int ITEMS_ON_PAGE = 15;

    private int _emptyNeed;

    [Parameter]
    public Player[]? Models { get; set; }

    public int TotalItemsCount { get; set; }
    public bool PreviousPageAvailable => Page > 1;
    public int Page { get; set; } = 1;
    public bool NextPageAvailable => Page < PagesCount;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;


    [Parameter]
    public string SearchString { get; set; } = string.Empty;

    private int PagesCount => Math.DivRem(TotalItemsCount, ITEMS_ON_PAGE, out int rem) + (rem > 0 ? 1 : 0);

    private void ToPlayerPage(int id)
    {
        NavigationManager.NavigateTo($"/player/{id}");
    }

    protected override async Task OnInitializedAsync()
    {
        await GetPlayers();
        await base.OnInitializedAsync();
    }

    private async Task ClearSearch()
    {
        SearchString = string.Empty;
        await GetPlayers();
        StateHasChanged();
    }

    private async Task OnKeyDownSearch(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter") await GetPlayers();
    }

    public async Task SetPage(int page)
    {
        Page = page;
        await GetPlayers();
        StateHasChanged();
    }

    public async Task GetPlayers(bool forceOnFirstPage = false)
    {
        if (forceOnFirstPage) Page = 1;
        using HttpResponseMessage response = await HttpClient.GetAsync(
            $"api/Player?skip={ITEMS_ON_PAGE * (Page - 1)}&take={ITEMS_ON_PAGE}&onlyClanMembers=true&search={UrlEncoder.Default.Encode(SearchString)}",
            HttpCompletionOption.ResponseHeadersRead);
        string? itemsCountString = response.Headers.FirstOrDefault(x =>
            string.Equals(x.Key, "Items-Count", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
        if (int.TryParse(itemsCountString, out int itemsCount)) TotalItemsCount = itemsCount;
        Models = await response.Content.ReadFromJsonAsync<Player[]>()
                 ?? throw new InvalidOperationException();
        _emptyNeed = ITEMS_ON_PAGE - Models.Length;
        StateHasChanged();
    }
}