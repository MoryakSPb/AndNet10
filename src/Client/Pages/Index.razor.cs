using System.Net.Http.Json;
using AndNet.Manager.Client.Shared;
using AndNet.Manager.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace AndNet.Manager.Client.Pages;

public partial class Index : ComponentBase
{
    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    public GlobalStats? GlobalStats { get; set; }

    [Parameter]
    public bool IsApplicationDisabled { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        IsApplicationDisabled = authState.User.Identity?.IsAuthenticated ?? false;
        if (!IsApplicationDisabled)
            IsApplicationDisabled =
                bool.Parse(
                    await JsRuntime.InvokeAsync<string?>("localStorage.getItem", "Application.IsSent") ?? "false");
        GlobalStats = await HttpClient.GetFromJsonAsync<GlobalStats>("api/GlobalStatistics", SerializationContext.Default.GlobalStats)
                      ?? throw new InvalidOperationException();
        MainLayout.Update();
        await base.OnInitializedAsync();
    }
}