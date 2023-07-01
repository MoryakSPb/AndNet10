using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Report;

[JsonPolymorphic]
[JsonDerivedType(typeof(ReportInfo), "О")]
[JsonDerivedType(typeof(ReportInfoExpedition), "ОЭ")]
[JsonDerivedType(typeof(ReportInfoBattle), "ОБ")]
public record ReportInfo : DocInfo
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}