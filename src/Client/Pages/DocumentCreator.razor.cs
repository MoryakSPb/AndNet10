using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models;
using AndNet.Manager.Shared.Models.Documentation;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Report;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Pages;

public partial class DocumentCreator : ComponentBase
{
    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    public bool IsAdvisor { get; set; }
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public ProtocolType ProtocolType { get; set; }

    public int SelectedType { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-1);
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(+1);
    public int ExpeditionId { get; set; }

    public string? EndPointRaw
    {
        get => EndPoint?.ToString();
        set => EndPoint = value is null ? null : IPEndPoint.TryParse(value, out IPEndPoint? result) ? result : null;
    }

    public IPEndPoint? EndPoint { get; set; } = new(IPAddress.Loopback, 27016);

    public ImmutableList<ReportInfoBattle.BattleCombatant> BattleCombatants { get; set; } =
        ImmutableList<ReportInfoBattle.BattleCombatant>.Empty
            .Add(new())
            .Add(new());

    public ImmutableSortedDictionary<int, Type> AvailableDocTypes => IsAdvisor
        ? new Dictionary<int, Type>
        {
            { 0, typeof(DocInfo) },
            { 1, typeof(DirectiveInfo) },
            { 2, typeof(ProtocolInfo) },
            { 3, typeof(ReportInfo) },
            { 4, typeof(ReportInfoBattle) },
            { 5, typeof(ReportInfoExpedition) },
            { 6, typeof(DecisionCouncil) },
            { 7, typeof(DecisionGeneralMeeting) }
        }.ToImmutableSortedDictionary()
        : new Dictionary<int, Type>
        {
            { 0, typeof(DocInfo) },
            { 1, typeof(DirectiveInfo) },
            { 3, typeof(ReportInfo) },
            { 4, typeof(ReportInfoBattle) },
            { 5, typeof(ReportInfoExpedition) }
        }.ToImmutableSortedDictionary();

    public bool CreateEnabled()
    {
        if (AvailableDocTypes[SelectedType] == typeof(ProtocolInfo)
            && ProtocolType == ProtocolType.Unknown)
            return false;
        if ((AvailableDocTypes[SelectedType] == typeof(ReportInfo)
             || AvailableDocTypes[SelectedType].BaseType == typeof(ReportInfo))
            && StartDate > EndDate) return false;
        if (AvailableDocTypes[SelectedType] == typeof(ReportInfoExpedition)
            && ExpeditionId == 0) return false;
        if (AvailableDocTypes[SelectedType] == typeof(ReportInfoBattle)
            && BattleCombatants.Any(x => string.IsNullOrWhiteSpace(x.Tag) || string.IsNullOrWhiteSpace(x.Name)))
            return false;
        return !string.IsNullOrWhiteSpace(Title);
    }

    public async Task Create()
    {
        DocWithBody doc = new()
        {
            Body = Body,
            Title = Title,
            Views = 0,
            CreationDate = DateTime.UtcNow,
            ParentId = null,
            Info = (DocInfo?)Activator.CreateInstance(AvailableDocTypes[SelectedType])
        };
        switch (doc.Info)
        {
            case ProtocolInfo protocolInfo:
                protocolInfo.ProtocolType = ProtocolType;
                break;
            case ReportInfoBattle reportInfoBattle:
                reportInfoBattle.StartDate = StartDate;
                reportInfoBattle.EndDate = EndDate;
                reportInfoBattle.Combatants = BattleCombatants;
                reportInfoBattle.ServerEndPoint = EndPoint;
                break;
            case ReportInfoExpedition reportInfoExpedition:
                reportInfoExpedition.StartDate = StartDate;
                reportInfoExpedition.EndDate = EndDate;
                reportInfoExpedition.ExpeditionId = ExpeditionId;
                break;
            case ReportInfo reportInfo:
                reportInfo.StartDate = StartDate;
                reportInfo.EndDate = EndDate;
                break;
        }

        Console.WriteLine(doc.ToString());
        string content = JsonSerializer.Serialize(doc);
        Console.WriteLine(content);
        HttpResponseMessage result = await HttpClient.PostAsync("api/document",
            new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));

        if (result.IsSuccessStatusCode)
        {
            int docId = await result.Content.ReadFromJsonAsync<int>();
            NavigationManager.NavigateTo($"document/{docId:D}");
        }
    }
}