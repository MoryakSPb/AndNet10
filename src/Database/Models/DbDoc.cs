using System.Collections.Immutable;
using System.Text.Json.Serialization;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models.Documentation;
using Markdig;

namespace AndNet.Manager.Database.Models;

public record DbDoc : Doc
{
    [JsonIgnore]
    public DbPlayer Author { get; set; } = null!;

    [JsonIgnore]
    public DbDoc? Parent { get; set; }

    [JsonIgnore]
    public DbDocBody? Body { get; set; }

    [JsonIgnore]
    public IList<DbDoc> Children { get; set; } = Array.Empty<DbDoc>();

    public override ImmutableArray<int> ChildIds
    {
        get => Children.Select(x => x.Id).ToImmutableArray();
        set => throw new NotSupportedException();
    }

    public DbDoc GenerateTitleFromBody(string body)
    {
        Title = Markdown.ToPlainText(body.Split(Environment.NewLine, 2)[0]).Trim();
        return this;
    }

    public DbDoc GenerateTitleFromBody()
    {
        GenerateTitleFromBody(Body!.Body);
        return this;
    }
}