using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared;

public partial class InfoBlock : ComponentBase
{
    [Parameter]
    public string Key { get; set; } = string.Empty;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string? Link { get; set; } = null;
}