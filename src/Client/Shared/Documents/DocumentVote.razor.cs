using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Client.Pages;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client.Shared.Documents;

public partial class DocumentVote : ComponentBase
{
    [Parameter]
    public int DocId { get; set; }

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Parameter]
    public Decision VoteDecision { get; set; } = null!;

    public bool VoteEnabled => VoteDecision.IsExecuted is null && VoteDecision.Votes.Any(x => x.PlayerId == MeId);
    public int? MeId { get; set; }

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

    [CascadingParameter]
    public Document? DocumentPage { get; set; }

    public bool IsAdvisor { get; set; }
    public bool ExecuteSupported { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetMeId();
        await PlayerNicknamesService.LoadNicknames(Enumerable.Empty<int>().Append(MeId!.Value)
            .Concat(VoteDecision.Votes.Select(x => x.PlayerId)));
        using HttpRequestMessage request = new(HttpMethod.Head, "api/auth/claims");
        using HttpResponseMessage response = await HttpClient.SendAsync(request);
        ExecuteSupported = response.StatusCode == HttpStatusCode.OK;
        await base.OnInitializedAsync();
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
            IsAdvisor = authState.User.IsInRole("advisor");
        }
        else
        {
            MeId = null;
        }
    }

    protected async Task Vote(VoteType voteType)
    {
        await HttpClient.PatchAsync($"api/Decision/{DocId}?vote={voteType:D}",
            new ByteArrayContent(Array.Empty<byte>()));
        await OnParametersSetAsync();
        StateHasChanged();
        if (DocumentPage is not null)
        {
            Doc newDoc = await HttpClient.GetFromJsonAsync<Doc>($"api/Document/{DocId}")
                         ?? throw new InvalidOperationException();
            DocumentPage.Doc = newDoc;
            DocumentPage.Update();
        }
    }

    protected async Task Execute(bool positive)
    {
        using HttpResponseMessage result = await HttpClient.GetAsync($"api/Decision/{DocId}/execute?agree={positive}");
        if (result.IsSuccessStatusCode) IsAdvisor = false;
        StateHasChanged();
        if (DocumentPage is not null)
        {
            Doc newDoc = await HttpClient.GetFromJsonAsync<Doc>($"api/Document/{DocId}")
                         ?? throw new InvalidOperationException();
            DocumentPage.Doc = newDoc;
            DocumentPage.Update();
        }
    }
}