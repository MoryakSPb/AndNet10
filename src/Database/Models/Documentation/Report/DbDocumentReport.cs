using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace AndNet.Manager.Database.Models.Documentation.Report;

public record DbDocumentReport : DbDocument
{
    public override string Prefix { get; set; } = "О";

    public DateTime StartDate
    {
        get => ReportRange.LowerBound;
        set => ReportRange = new(value, ReportRange.UpperBound);
    }

    public DateTime EndDate
    {
        get => ReportRange.LowerBound;
        set => ReportRange = new(ReportRange.LowerBound, value);
    }

    [JsonIgnore]
    public NpgsqlRange<DateTime> ReportRange { get; set; }
}