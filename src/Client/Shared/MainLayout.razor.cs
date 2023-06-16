using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared;

public partial class MainLayout : LayoutComponentBase
{
    protected RenderFragment Aside = null!;
    protected RenderFragment? Footer;
    protected RenderFragment Header = null!;

    public void SetFragmentsAndUpdate(RenderFragment header, RenderFragment aside, RenderFragment? footer)
    {
        Footer = footer;
        Header = header;
        Aside = aside;
        StateHasChanged();
    }

    public void Update()
    {
        StateHasChanged();
    }
}