using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared;

public class LayoutSetter : ComponentBase
{
    [CascadingParameter]
    public MainLayout Layout { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Aside { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    protected override void OnInitialized()
    {
        Layout?.SetFragmentsAndUpdate(Header, Aside, Footer);
    }
}