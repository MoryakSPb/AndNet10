using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public sealed class DecisionCouncilPlayerAwardSheetStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;

    public DecisionCouncilPlayerAwardSheetStrategy(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is not DecisionCouncilPlayerAwardSheet info) throw new InvalidOperationException();
        DbPlayer target =
            await _databaseContext.Players.Include(x => x.Awards).FirstOrDefaultAsync(x => x.Id == info.PlayerId)
                .ConfigureAwait(false)
            ?? throw new ArgumentOutOfRangeException(nameof(doc));
        await _databaseContext.Awards.AddAsync(new()
        {
            AwardSheetId = doc.Id,
            AutomationId = info.AutomationId,
            AwardType = info.AwardType,
            PlayerId = info.PlayerId,
            IsMarkedForDelete = false,
            IssueDate = info.PredeterminedIssueDate?.ToLocalTime() ?? DateTime.UtcNow,
            IssuerId = doc.AuthorId,
            Description = info.Description
        }).ConfigureAwait(false);
        if (target is DbClanPlayer clanPlayer) clanPlayer.CalcPlayer();
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
    }
}