using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public class DecisionCouncilExpeditionStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;

    public DecisionCouncilExpeditionStrategy(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is DecisionCouncilExpeditionCreate createInfo)
        {
            DateTime now = DateTime.UtcNow;
            await _databaseContext.Expeditions.AddAsync(new()
            {
                AccountablePlayerId = createInfo.AccountablePlayerId,
                DiscordRoleId = null,
                During = new(now, now.Add(createInfo.Duration)),
                IsMarkedForDelete = false,
                Members = _databaseContext.Players.Join(createInfo.Members, x => x.Id, x => x, (player, id) => player)
                    .ToList()
            }).ConfigureAwait(false);
            await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
            return;
        }

        if (doc.Info is not DecisionCouncilExpedition info) throw new InvalidOperationException();
        DbExpedition expedition = await _databaseContext.Expeditions.FirstOrDefaultAsync(x => x.Id == info.ExpeditionId)
                                      .ConfigureAwait(false)
                                  ?? throw new ArgumentOutOfRangeException(nameof(doc));
        switch (info)
        {
            case DecisionCouncilExpeditionClose:
                expedition.EndDate = DateTime.UtcNow;
                break;
            case DecisionCouncilExpeditionProlongation prolongationInfo:
                expedition.EndDate = expedition.EndDate.Add(prolongationInfo.ProlongationTime);
                break;
            case DecisionCouncilExpeditionPlayer playerInfo:
                DbPlayer player = await _databaseContext.Players.FirstOrDefaultAsync(x => x.Id == playerInfo.PlayerId)
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
    }
}