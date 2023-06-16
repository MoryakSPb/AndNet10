namespace AndNet.Manager.Database.Models.Documentation.Report;

public record DbDocumentReportExpedition : DbDocumentReport
{
    public override string Prefix { get; set; } = "ОЭ";
    public int ExpeditionId { get; set; }
    public DbExpedition Expedition { get; set; } = null!;
}