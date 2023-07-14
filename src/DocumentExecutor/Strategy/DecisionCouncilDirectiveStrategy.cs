using AndNet.Integration.Discord.Services;
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
    private readonly DiscordService _discordService;

    public DecisionCouncilDirectiveStrategy(DatabaseContext databaseContext, DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _discordService = discordService;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is not DecisionCouncilDirective info) throw new InvalidOperationException();
        DbDoc directive = await _databaseContext.Documents.FirstOrDefaultAsync(x => x.Id == info.DirectiveId)
                              .ConfigureAwait(false)
                          ?? throw new ArgumentOutOfRangeException(nameof(doc));
        if (directive.Info is not DirectiveInfo directiveInfo) throw new InvalidOperationException();
        DbDoc newDirective = null!;
        if (info is DecisionCouncilDirectiveChange directiveChange)
        {
            newDirective = await _databaseContext.Documents
                .FirstOrDefaultAsync(x => x.Id == directiveChange.NewDirectiveId)
                .ConfigureAwait(false) ?? throw new ArgumentOutOfRangeException(nameof(doc));
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

        if (info is DecisionCouncilDirectiveChange directiveChangeLog)
            await _discordService.SendBotLogMessageAsync(
                $"Директива №{directive.Id} «{directive.Title}» заменена на диркетиву №{directiveChangeLog.NewDirectiveId} «{newDirective.Title}». Рекомендуется ознакомится с содержанием заменяющей директивы: https://andromeda-se.xyz/document/{directive.Id}"
                + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
        else
            switch (info.Action)
            {
                case DecisionCouncilDirective.DirectiveAction.Generic:

                    break;
                case DecisionCouncilDirective.DirectiveAction.Accept:
                    await _discordService.SendBotLogMessageAsync(
                        $"Директива №{directive.Id} «{directive.Title}» теперь является действующей. Рекомендуется ознакомится с её содержанием: https://andromeda-se.xyz/document/{directive.Id}"
                        + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                    break;
                case DecisionCouncilDirective.DirectiveAction.Cancel:
                    await _discordService.SendBotLogMessageAsync(
                        $"Действие директивы №{directive.Id} «{directive.Title}» отменено."
                        + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }
}