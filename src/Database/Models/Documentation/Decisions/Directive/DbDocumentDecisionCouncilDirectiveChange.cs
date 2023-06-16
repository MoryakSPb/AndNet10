﻿using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Directive;

public record DbDocumentDecisionCouncilDirectiveChange : DbDocumentDecisionCouncilDirective
{
    public override string Prefix { get; set; } = "РСДЗ";
    public int NewDirectiveId { get; set; }
    public DbDocumentDirective NewDirective { get; set; } = null!;

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Directive.CancelDate = DateTime.UtcNow;
        Directive.ReplacedBy = NewDirective;
        Directive.ReplacedById = NewDirectiveId;
    }
}