using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public sealed class DecisionCouncilPlayerAwardSheetStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;

    public DecisionCouncilPlayerAwardSheetStrategy(DatabaseContext databaseContext, DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _discordService = discordService;
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
        string newAwardMessage = string.Empty;
        if (target is DbClanPlayer clanPlayer)
        {
            PlayerRank oldRank = clanPlayer.Rank;
            clanPlayer.CalcPlayer();
            PlayerRank newRank = clanPlayer.Rank;
            if (oldRank != newRank)
                newAwardMessage =
                    $"{Environment.NewLine}<@{target.DiscordId:D}> {(newRank > oldRank ? "повышен(а)" : "понижен(а)")} в ранге до [{RankRules.Icons[newRank]}] «{RankRules.Names[newRank]}»";
        }

        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        await _discordService.SendBotLogMessageAsync(
            $"Игрок <@{target.DiscordId:D}> получает «{AwardRules.Names[info.AwardType]}», описание гласит: *{info.Description}*"
            + newAwardMessage
            + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
    }
}