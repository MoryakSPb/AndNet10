using System.Text.Json.Serialization;

namespace AndNet.Manager.Shared.Models.Documentation.Info;

[JsonDerivedType(typeof(DirectiveInfo), "Д")]
public record DirectiveInfo : DocInfo
{
    public enum DirectiveStatus
    {
        Canceled = -2,
        Replaced = -1,
        Project = 0,
        Accepted = 1
    }

    public DateTime? CancelDate { get; set; }

    public DateTime? AcceptanceDate { get; set; }

    public int? ReplacedById { get; set; }

    [JsonIgnore]
    public DirectiveStatus Status
    {
        get
        {
            if (ReplacedById is not null) return DirectiveStatus.Replaced;
            if (CancelDate is not null) return DirectiveStatus.Canceled;
            if (AcceptanceDate is not null) return DirectiveStatus.Accepted;
            return DirectiveStatus.Project;
        }
    }
}