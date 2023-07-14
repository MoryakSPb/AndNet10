using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Interfaces;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

public partial class Document : ComponentBase
{
    private int _prevId;

    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public bool LoadBody { get; set; } = true;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    private RenderFragment Body { get; set; } = builder => builder.AddMarkupContent(0, "<h1>Загрузка…</h1>");

    public Doc? Doc { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Id == _prevId) return;
        _prevId = Id;
        Doc doc = await HttpClient.GetFromJsonAsync<Doc>($"api/Document/{Id}")
                  ?? throw new InvalidOperationException();

        if (LoadBody)
        {
            string markup = await HttpClient.GetStringAsync($"api/Document/{Id}/body")
                            ?? throw new InvalidOperationException();
            Body = builder => builder.AddMarkupContent(0, markup);
        }

        IEnumerable<int> playersIds = Enumerable.Empty<int>();
        playersIds = playersIds.Append(doc.AuthorId);
        if (doc.Info is Decision decision)
        {
            if (decision.ExecutorId is not null) playersIds = playersIds.Append(decision.ExecutorId.Value);
            playersIds = playersIds.Concat(decision.Votes.Select(x => x.PlayerId));
            if (doc.Info is IPlayerId playerId) playersIds = playersIds.Append(playerId.PlayerId);
        }
        else if (doc.Info is ProtocolInfo protocolInfo)
        {
            playersIds = playersIds.Concat(protocolInfo.Members);
        }

        await PlayerNicknamesService.LoadNicknames(playersIds);

        Doc = doc;
        Update();
        await base.OnParametersSetAsync();
    }

    public void Update()
    {
        MainLayout.Update();
        StateHasChanged();
    }
}