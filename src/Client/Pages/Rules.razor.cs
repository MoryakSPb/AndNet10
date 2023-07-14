using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

public partial class Rules
{
    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    public RenderFragment RulesMarkup { get; set; }

    protected override async Task OnInitializedAsync()
    {
        string rulesHtml = await HttpClient.GetStringAsync("static/rules.html");
        RulesMarkup = builder => builder.AddMarkupContent(0, rulesHtml);
        await base.OnInitializedAsync();
    }
}