using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Election;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

[Authorize(Roles = "member")]
public partial class Elections : ComponentBase
{
    private readonly Dictionary<int, string> _nicknames = new();
    public Election? Election { get; set; }

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    public bool IsVoted { get; set; }

    [CascadingParameter]
    public MainLayout MainLayout { get; set; }

    private Dictionary<int, bool?>? Bulletin { get; set; }

    private Player Me { get; set; }

    protected override async Task OnParametersSetAsync()
    {
    }

    protected override async Task OnInitializedAsync()
    {
        Me = await HttpClient.GetFromJsonAsync<Player>("api/Player/me")
             ?? throw new InvalidOperationException();

        Election election = await HttpClient.GetFromJsonAsync<Election>("api/Election")
                            ?? throw new InvalidOperationException();
        election.ElectionEnd =
            DateTime.SpecifyKind(election.ElectionEnd, DateTimeKind.Utc).ToUniversalTime().ToLocalTime();
        if (election.Stage == ElectionStage.Voting)
        {
            IsVoted = await HttpClient.GetFromJsonAsync<bool>("api/Election/isVoted");
            Bulletin = election.Candidates.ToDictionary(x => x.PlayerId, _ => default(bool?));
        }

        foreach (ElectionCandidate electionCandidate in election.Candidates)
            if (!_nicknames.ContainsKey(electionCandidate.PlayerId))
                _nicknames.TryAdd(electionCandidate.PlayerId,
                    await HttpClient.GetStringAsync($"api/Player/{electionCandidate.PlayerId}/nickname"));
        Election = election;
    }

    public async Task Reg()
    {
        await HttpClient.PatchAsync("api/Election", new ReadOnlyMemoryContent(ReadOnlyMemory<byte>.Empty));
        await OnInitializedAsync();
        StateHasChanged();
        MainLayout.Update();
    }

    public async Task Vote()
    {
        await HttpClient.PostAsJsonAsync("api/Election", Bulletin);
        await OnInitializedAsync();
        StateHasChanged();
        MainLayout.Update();
    }
}