using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models.Documentation.Info;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public class DecisionCouncilDirectiveStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;

    public DecisionCouncilDirectiveStrategy(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is not DecisionCouncilDirective info) throw new InvalidOperationException();
        DbDoc directive = await _databaseContext.Documents.FirstOrDefaultAsync(x => x.Id == info.DirectiveId)
                              .ConfigureAwait(false)
                          ?? throw new ArgumentOutOfRangeException(nameof(doc));
        if (directive.Info is not DirectiveInfo directiveInfo) throw new InvalidOperationException();

        if (info is DecisionCouncilDirectiveChange directiveChange)
        {
            directiveInfo.CancelDate = DateTime.UtcNow;
            directiveInfo.ReplacedById = directiveChange.NewDirectiveId;
        }
        else
        {
            switch (info.Action)
            {
                case DecisionCouncilDirective.DirectiveAction.Accept:
                    directiveInfo.CancelDate = null;
                    directiveInfo.AcceptanceDate = DateTime.UtcNow;
                    directiveInfo.ReplacedById = null;
                    break;
                case DecisionCouncilDirective.DirectiveAction.Cancel:
                    directiveInfo.CancelDate = DateTime.UtcNow;
                    break;
                case DecisionCouncilDirective.DirectiveAction.Generic:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        directive.Info = directiveInfo;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
    }
}