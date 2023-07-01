using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client.Pages;

public partial class Document : ComponentBase
{
    protected readonly Dictionary<int, string> VotersNicknames = new();
    private int _prevId;

    [Parameter]
    public int Id { get; set; }

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    private RenderFragment Body { get; set; } = builder => builder.AddMarkupContent(0, "<h1>Загрузка…</h1>");
    public Doc? Doc { get; set; }
    public string AuthorNickname { get; set; }
    public bool VoteEnabled => Doc?.Info is Decision decision && decision.Votes.Any(x => x.PlayerId == MeId);

    public int? MeId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Id == _prevId) return;
        _prevId = Id;
        Doc doc = await HttpClient.GetFromJsonAsync<Doc>($"api/Document/{Id}")
                  ?? throw new InvalidOperationException();
        AuthorNickname = await HttpClient.GetStringAsync($"api/Player/{doc.AuthorId}/nickname")
                         ?? throw new InvalidOperationException();

        string markup = await HttpClient.GetStringAsync($"api/Document/{Id}/body")
                        ?? throw new InvalidOperationException();
        Body = builder => builder.AddMarkupContent(0, markup);

        if (doc.Info is Decision decisionInfo)
            foreach (Decision.Vote vote in decisionInfo.Votes.ExceptBy(VotersNicknames.Keys, vote => vote.PlayerId))
            {
                string? nickname = await HttpClient.GetStringAsync($"api/Player/{vote.PlayerId}/nickname");
                VotersNicknames.Add(vote.PlayerId, nickname ?? string.Empty);
            }

        await GetMeId();
        Doc = doc;
        MainLayout.Update();
        StateHasChanged();
        await base.OnParametersSetAsync();
    }

    protected async Task Vote(VoteType voteType)
    {
        await HttpClient.PatchAsync($"api/Decision/{Id}?vote={voteType:D}", new ByteArrayContent(Array.Empty<byte>()));
        _prevId = 0;
        await OnParametersSetAsync();
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
    }
}