using NpgsqlTypes;

namespace AndNet.Manager.Database.Models;

public record DbDocBody
{
    public int DocId { get; set; }
    public DbDoc Doc { get; set; }
    public string Body { get; set; } = string.Empty;
    public NpgsqlTsVector SearchVector { get; set; }
    public uint Version { get; set; }
}