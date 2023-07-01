using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info.Report;

[JsonPolymorphic]
[JsonDerivedType(typeof(ReportInfoExpedition), "ОЭ")]
public record ReportInfoExpedition : ReportInfo
{
    public int ExpeditionId { get; set; }
}