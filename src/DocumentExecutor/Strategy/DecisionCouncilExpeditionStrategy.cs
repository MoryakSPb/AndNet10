using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public class DecisionCouncilExpeditionStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;

    public DecisionCouncilExpeditionStrategy(DatabaseContext databaseContext, DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _discordService = discordService;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is DecisionCouncilExpeditionCreate createInfo)
        {
            DateTime now = DateTime.UtcNow;
            EntityEntry<DbExpedition> result = await _databaseContext.Expeditions.AddAsync(new()
            {
                AccountablePlayerId = createInfo.AccountablePlayerId,
                DiscordRoleId = null,
                During = new(now, now.Add(createInfo.Duration)),
                IsMarkedForDelete = false,
                Members = createInfo.Members.Join(
                        await _databaseContext.Players.ToArrayAsync().ConfigureAwait(false),
                        x => x, 
                        x => x.Id, 
                        (id, player) => player)
                    .ToList()
            }).ConfigureAwait(false);

            await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
            await _discordService.CreateExpeditionRoleAsync(result.Entity.Id).ConfigureAwait(false);
            await _discordService.CreateExpeditionsChannels(result.Entity.Id).ConfigureAwait(false);
            await _discordService.SendBotLogMessageAsync($"Создана новая экспедиция №{result.Entity.Id:D}!"
                                                         + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
            return;
        }

        if (doc.Info is not DecisionCouncilExpedition info) throw new InvalidOperationException();
        DbExpedition expedition = await _databaseContext.Expeditions.FirstOrDefaultAsync(x => x.Id == info.ExpeditionId)
                                      .ConfigureAwait(false)
                                  ?? throw new ArgumentOutOfRangeException(nameof(doc));
        DbPlayer player = null!;
        switch (info)
        {
            case DecisionCouncilExpeditionClose:
                expedition.EndDate = DateTime.UtcNow;
                break;
            case DecisionCouncilExpeditionProlongation prolongationInfo:
                expedition.EndDate = expedition.EndDate.Add(prolongationInfo.ProlongationTime);
                break;
            case DecisionCouncilExpeditionPlayer playerInfo:
                player = await _databaseContext.Players.FirstOrDefaultAsync(x => x.Id == playerInfo.PlayerId)
                             .ConfigureAwait(false)
                         ?? throw new ArgumentOutOfRangeException(nameof(doc));
                switch (playerInfo.Action)
                {
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Unknown:
                        break;
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Add:
                        expedition.Members.Add(player);
                        break;
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Remove:
                        expedition.Members.Remove(player);
                        break;
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.ChangeCommander:
                        expedition.AccountablePlayer = player;
                        expedition.AccountablePlayerId = playerInfo.PlayerId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
        }

        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        switch (info)
        {
            case DecisionCouncilExpeditionClose:
                await _discordService.SendBotLogMessageAsync($"Экспедиция №{expedition.Id:D} досрочно распущена"
                                                             + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                break;
            case DecisionCouncilExpeditionProlongation prolongationInfo:
                await _discordService.SendBotLogMessageAsync(
                    $"Экспедиция №{expedition.Id:D} продлена на {prolongationInfo.ProlongationTime.TotalDays:F0} суток"
                    + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                break;
            case DecisionCouncilExpeditionPlayer playerInfo:
                switch (playerInfo.Action)
                {
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Unknown:
                        break;
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Add:
                        await _discordService.SendBotLogMessageAsync(
                            $"<@{player.DiscordId:D}> теперь входит в состав экспедиции №{expedition.Id:D}"
                            + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                        break;
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.Remove:
                        await _discordService.SendBotLogMessageAsync(
                            $"<@{player.DiscordId:D}> более не является участником экспедиции №{expedition.Id:D}"
                            + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                        break;
                    case DecisionCouncilExpeditionPlayer.ExpeditionPlayerAction.ChangeCommander:
                        await _discordService.SendBotLogMessageAsync(
                            $"<@{player.DiscordId:D}> — новый командир экспедиции №{expedition.Id:D}"
                            + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
        }
    }
}