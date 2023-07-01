using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AndNet.Manager.Client.Pages;

public partial class Documents : ComponentBase
{
    public const int DOCS_ITEMS_ON_PAGE = 5;
    private readonly Dictionary<int, string> _players = new();
    private DocumentCategory _category = DocumentCategory.All;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Parameter]
    public Doc[]? Docs { get; set; }

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    public int DocsTotalItemsCount { get; set; }
    public bool DocsPreviousPageAvailable => DocsPage > 1;
    public int DocsPage { get; set; } = 1;
    public bool DocsNextPageAvailable => DocsPage < DocsPagesCount;
    private int DocsPagesCount => Math.DivRem(DocsTotalItemsCount, DOCS_ITEMS_ON_PAGE, out int rem) + (rem > 0 ? 1 : 0);

    public DocumentSortType SortType { get; set; } = DocumentSortType.CreationDate;

    public DocumentCategory Category
    {
        get => _category;
        set
        {
            _category = value;
            StateHasChanged();
            MainLayout.Update();
        }
    }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string SearchString { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadDocs();
    }

    public async Task LoadDocs(bool resetPage = false)
    {
        Docs = null;
        StateHasChanged();
        if (resetPage) DocsPage = 1;
        string query =
            $"api/Document?skip={DOCS_ITEMS_ON_PAGE * (DocsPage - 1)}&take={DOCS_ITEMS_ON_PAGE}";
        query += $"&sortType={SortType:D}";
        query += $"&category={Category:D}";
        query += $"&searchString={SearchString}";
        if (StartDate.HasValue)
            query += $"&start={(long)Math.Truncate((StartDate.Value - DateTime.UnixEpoch).TotalSeconds)}";
        if (EndDate.HasValue) query += $"&end={(long)Math.Truncate((EndDate.Value - DateTime.UnixEpoch).TotalSeconds)}";
        HttpResponseMessage response = await HttpClient.GetAsync(query)
                                       ?? throw new InvalidOperationException();
        string? itemsCountString = response.Headers.FirstOrDefault(x =>
            string.Equals(x.Key, "Items-Count", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
        if (int.TryParse(itemsCountString, out int itemsCount)) DocsTotalItemsCount = itemsCount;
        Docs = await response.Content.ReadFromJsonAsync<Doc[]>();
        await LoadDocAuthors();
        StateHasChanged();
        MainLayout.Update();
    }

    private async Task OnKeyDownSearch(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter") await LoadDocs(true);
    }

    private async Task LoadDocAuthors()
    {
        foreach (int playerId in Docs!.Select(x => x.AuthorId).Except(_players.Keys).Distinct())
        {
            if (_players.ContainsKey(playerId)) continue;
            string nickname = await HttpClient.GetStringAsync($"api/Player/{playerId}/nickname");
            _players.Add(playerId, nickname);
        }
    }

    private async Task ClearSearch()
    {
        SearchString = string.Empty;
        await SetDocPage(1);
    }

    public async Task SetDocPage(int page)
    {
        DocsPage = page;
        await LoadDocs();
    }
}