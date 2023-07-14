using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Models;
using BlazorBootstrap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

[Authorize(Roles = "member")]
public partial class Expeditions : ComponentBase
{
    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

    [Parameter]
    public Expedition[]? Models { get; set; }

    [Parameter]
    public bool ShowInactive { get; set; }

    public Modal Modal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Models = await HttpClient.GetFromJsonAsync<Expedition[]>("api/Expedition?getDeleted=true")
                 ?? throw new InvalidOperationException();
        await PlayerNicknamesService.LoadNicknames(Models.Select(x => x.CommanderId));
    }

    private void ToExpeditionPage(int id)
    {
        NavigationManager.NavigateTo($"/expedition/{id}");
    }
}