using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared.Documents;

public partial class DocumentCard : ComponentBase
{
    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;


    [Parameter]
    public string SearchString { get; set; } = string.Empty;

    [Parameter]
    public Doc Document { get; set; }

    public string AuthorNickname { get; set; }
    public string ExecutorNickname { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        using HttpResponseMessage authorResponse =
            await HttpClient.GetAsync($"api/Player/{Document.AuthorId}/nickname");
        if (!authorResponse.IsSuccessStatusCode) throw new InvalidOperationException();
        AuthorNickname = await authorResponse.Content.ReadAsStringAsync();
        if (Document.Info is Decision decisionInfo)
            if (decisionInfo.ExecutorId.HasValue)
            {
                using HttpResponseMessage executorResponse =
                    await HttpClient.GetAsync($"api/Player/{Document.AuthorId}/nickname");
                if (!executorResponse.IsSuccessStatusCode) throw new InvalidOperationException();
                ExecutorNickname = await executorResponse.Content.ReadAsStringAsync();
            }

        Console.WriteLine($"{nameof(Document.Info.Type)}: {Document.Info?.Type ?? "null"}");
    }
}