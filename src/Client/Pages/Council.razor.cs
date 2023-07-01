using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

public partial class Council : ComponentBase
{
    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    private int[]? DocIds { get; set; }

    protected override async Task OnInitializedAsync()
    {
        DocIds = await HttpClient.GetFromJsonAsync<int[]>("api/Decision").ConfigureAwait(false);
        await base.OnInitializedAsync();
    }
}