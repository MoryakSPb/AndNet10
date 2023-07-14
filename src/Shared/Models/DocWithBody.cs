using AndNet.Manager.Shared.Models.Documentation;

namespace AndNet.Manager.Shared.Models;

public record DocWithBody : Doc
{
    public string Body { get; set; } = string.Empty;
}