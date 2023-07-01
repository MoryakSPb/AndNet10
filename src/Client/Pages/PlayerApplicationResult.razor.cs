using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

public partial class PlayerApplicationResult : ComponentBase
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "text")]
    public string Text { get; set; }
}