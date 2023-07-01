using System.Globalization;
using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using ApexCharts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace AndNet.Manager.Client.Pages;

public partial class PlayerPage : ComponentBase
{
    public const int DOCS_ITEMS_ON_PAGE = 10;
    private readonly Dictionary<int, Player> _players = new();
    private Award[]? _awards;
    private Expedition[]? _expeditions;
    private PlayerLeaveReason _leaveReason = 0;
    private string _newAwardDescription = string.Empty;
    private AwardType _newAwardType = 0;

    private int prevId;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    public bool IsAdvisor { get; set; }

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    [Parameter]
    public Player? Model { get; set; }

    [Parameter]
    public int Id { get; set; }

    public bool ShowPlayerEditor { get; set; }

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    public TimeZoneInfo? PlayerTimeZone { get; set; }
    public int StatsDays { get; set; } = 7;
    public Dictionary<DateTime, PlayerStatisticsStatus>? Stats { get; set; }

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    public string TimeZoneOffset
    {
        get
        {
            TimeSpan? offset = PlayerTimeZone?.GetUtcOffset(DateTime.UtcNow);
            return $"{(offset < TimeSpan.Zero ? "-" : "+")}{offset:hh\\:mm}";
        }
    }

    public string? TimeZoneOffsetFromLocal
    {
        get
        {
            if (PlayerTimeZone is null) return null;
            DateTime now = DateTime.UtcNow;
            TimeSpan playerOffset = PlayerTimeZone.GetUtcOffset(now);
            TimeSpan localOffset = TimeZoneInfo.Local.GetUtcOffset(now);
            TimeSpan result = playerOffset - localOffset;
            return result == TimeSpan.Zero ? "±00:00" : $"{(result < TimeSpan.Zero ? "-" : "+")}{result:hh\\:mm}";
        }
    }

    private IEnumerable<(AwardType Key, int)> GetCountsByType
    {
        get
        {
            Dictionary<AwardType, int> values =
                _awards!.GroupBy(x => x.AwardType).ToDictionary(x => x.Key, x => x.Count());
            return Enum.GetValues<AwardType>().OrderByDescending(x => (int)x)
                .Select(x => (x, values.GetValueOrDefault(x, 0)));
        }
    }

    public AwardType NewAwardType
    {
        get => _newAwardType;
        set
        {
            _newAwardType = value;
            MainLayout.Update();
        }
    }

    public string NewAwardDescription
    {
        get => _newAwardDescription;
        set
        {
            _newAwardDescription = value;
            MainLayout.Update();
        }
    }

    public PlayerLeaveReason LeaveReason
    {
        get => _leaveReason;
        set
        {
            _leaveReason = value;
            MainLayout.Update();
        }
    }

    private Doc[]? Docs { get; set; }
    public int DocsTotalItemsCount { get; set; }
    public bool DocsPreviousPageAvailable => DocsPage > 1;
    public int DocsPage { get; set; } = 1;
    public bool DocsNextPageAvailable => DocsPage < DocsPagesCount;
    private int DocsPagesCount => Math.DivRem(DocsTotalItemsCount, DOCS_ITEMS_ON_PAGE, out int rem) + (rem > 0 ? 1 : 0);

    public ApexChartOptions<Tuple<DateTime, PlayerStatisticsStatus>> Options { get; } = new()
    {
        Tooltip = new()
        {
            X = new()
            {
                Show = true,
                Format =
                    $"{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern} {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}"
            }
        },
        Xaxis = new()
        {
            Labels = new()
            {
                DatetimeUTC = false
            }
        }
    };

    public async Task CreateAwardSheet()
    {
        await HttpClient.PatchAsync(
            $"api/player/{Id}/award?awardType={NewAwardType:D}&description={Uri.EscapeDataString(NewAwardDescription)}",
            new ByteArrayContent(Array.Empty<byte>()));
        NewAwardType = 0;
        NewAwardDescription = string.Empty;
        StateHasChanged();
    }

    public async Task CreateReserve()
    {
        await HttpClient.PatchAsync(
            $"api/player/{Id}/reserve",
            new ByteArrayContent(Array.Empty<byte>()));
    }

    public async Task CreateKick()
    {
        await HttpClient.DeleteAsync($"api/player/{Id}?leaveReason={LeaveReason:D}");
        LeaveReason = 0;
        StateHasChanged();
    }

    private async Task LoadAwardIssuers()
    {
        _players.Clear();
        foreach (int playerId in _awards!.Where(x => x.IssuerId.HasValue).Select(x => x.IssuerId!.Value).Distinct())
        {
            Player? player = await HttpClient.GetFromJsonAsync<Player>($"api/Player/{playerId}");
            _players.Add(playerId, player);
        }
    }

    private async Task GetAwards()
    {
        if (_awards is not null) return;
        _awards = await HttpClient.GetFromJsonAsync<Award[]>($"api/award?playerId={Id}");
        await LoadAwardIssuers();
    }

    private async Task GetDocs()
    {
        using HttpResponseMessage response =
            await HttpClient.GetAsync(
                $"api/Player/{Id}/docs?skip={DOCS_ITEMS_ON_PAGE * (DocsPage - 1)}&take={DOCS_ITEMS_ON_PAGE}");
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

    protected override async Task OnInitializedAsync()
    {
        prevId = 0;
        await base.OnInitializedAsync();
    }

    private static string FormatYAxisLabel(decimal arg)
    {
        return arg - Math.Truncate(arg) != 0m
            ? string.Empty
            : PlayerRules.PlayerStatisticsStatusNames.GetValueOrDefault((PlayerStatisticsStatus)(int)arg, string.Empty);
    }

    protected async Task GetStats()
    {
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Player/{Id}/stats");
        if (!response.IsSuccessStatusCode) return;
        Stats = await response.Content.ReadFromJsonAsync<Dictionary<DateTime, PlayerStatisticsStatus>>()
                ?? throw new InvalidOperationException();
        StateHasChanged();
    }

    private static string GetPointChartColor(Tuple<DateTime, PlayerStatisticsStatus> arg)
    {
        return arg.Item2 switch
        {
            PlayerStatisticsStatus.Offline => "#8a9198",
            PlayerStatisticsStatus.Online => "#0d6efd",
            PlayerStatisticsStatus.InDifferentGame => "#ffc107",
            PlayerStatisticsStatus.InSpaceEngineers => "#198754",
            PlayerStatisticsStatus.InSpaceEngineersWithComrade => "#6f42c1",
            PlayerStatisticsStatus.Unknown => "#dc3545",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Id == prevId) return;
        prevId = Id;
        Model = await HttpClient.GetFromJsonAsync<Player>($"api/Player/{Id}");
        PlayerTimeZone = Model?.TimeZone is null ? null : TimeZoneInfo.FindSystemTimeZoneById(Model.TimeZone);
        _expeditions =
            await HttpClient.GetFromJsonAsync<Expedition[]>(
                $"api/Expedition?playerId={Id}&getDeleted=true&skip=0&take={int.MaxValue}")
            ?? throw new InvalidOperationException();
        _expeditions = _expeditions
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CommanderId == Id)
            .ThenByDescending(x => x.StartDate)
            .ThenByDescending(x => x.EndDate)
            .ToArray();

        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        ShowPlayerEditor = (authState.User.Identity?.IsAuthenticated ?? false)
                           && int.TryParse(
                               authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                               out int playerId)
                           && Id == playerId;
        IsAdvisor = authState.User.IsInRole("advisor");
        await GetStats();
        StateHasChanged();
        MainLayout.Update();
        _awards = null;
        await GetAwards();
        await SetDocPage(1);
    }
}