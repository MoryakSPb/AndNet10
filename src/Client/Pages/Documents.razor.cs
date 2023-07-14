using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AndNet.Manager.Client.Pages;

public partial class Documents : ComponentBase
{
    public const int DOCS_ITEMS_ON_PAGE = 5;
    private DocumentCategory _category = DocumentCategory.All;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

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

    public int? DecisionStatus { get; set; }
    public int? DirectiveStatus { get; set; }

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
        if (Category == DocumentCategory.Decision && DecisionStatus is not null)
            query += $"&decisionStatus={DecisionStatus:D}";
        if (Category == DocumentCategory.Directive && DirectiveStatus is not null)
            query += $"&directiveStatus={DirectiveStatus:D}";
        if (StartDate.HasValue)
            query += $"&start={(long)Math.Truncate((StartDate.Value - DateTime.UnixEpoch).TotalSeconds)}";
        if (EndDate.HasValue) query += $"&end={(long)Math.Truncate((EndDate.Value - DateTime.UnixEpoch).TotalSeconds)}";
        using HttpResponseMessage response = await HttpClient.GetAsync(query)
                                             ?? throw new InvalidOperationException();
        string? itemsCountString = response.Headers.FirstOrDefault(x =>
            string.Equals(x.Key, "Items-Count", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
        if (int.TryParse(itemsCountString, out int itemsCount)) DocsTotalItemsCount = itemsCount;
        Doc[] docs = await response.Content.ReadFromJsonAsync<Doc[]>(SerializationContext.Default.DocArray) ?? throw new InvalidOperationException();
        await PlayerNicknamesService.LoadNicknames(docs.Select(x => x.AuthorId));
        foreach (Doc doc in docs)
        {
            switch (doc.Info)
            {
                case Decision decision:
                    await PlayerNicknamesService.LoadNicknames(decision.Votes.Select(x => x.PlayerId));
                    break;
                case ProtocolInfo protocol:
                    await PlayerNicknamesService.LoadNicknames(protocol.Members);
                    break;
            }

            if (doc.Info is IPlayerId playerId)
                await PlayerNicknamesService.LoadNicknames(Enumerable.Empty<int>().Append(playerId.PlayerId));
        }

        Docs = docs;
        StateHasChanged();
        MainLayout.Update();
    }

    private async Task OnKeyDownSearch(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter") await LoadDocs(true);
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

    public async Task RestoreFilters()
    {
        SearchString = string.Empty;
        SortType = DocumentSortType.CreationDate;
        Category = DocumentCategory.All;
        DecisionStatus = null;
        DirectiveStatus = null;
        StartDate = null;
        EndDate = null;
        await SetDocPage(1);
    }

    public async Task DirectiveFilter()
    {
        SearchString = string.Empty;
        SortType = DocumentSortType.Views;
        Category = DocumentCategory.Directive;
        DecisionStatus = null;
        DirectiveStatus = 1;
        StartDate = null;
        EndDate = null;
        await SetDocPage(1);
    }

    public async Task DecisionFilter()
    {
        SearchString = string.Empty;
        SortType = DocumentSortType.Views;
        Category = DocumentCategory.Decision;
        DecisionStatus = 0;
        DirectiveStatus = null;
        StartDate = null;
        EndDate = null;
        await SetDocPage(1);
    }
}