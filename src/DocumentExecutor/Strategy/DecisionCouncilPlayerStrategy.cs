using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public class DecisionCouncilPlayerStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;
    private readonly UserManager<DbUser> _userManager;

    public DecisionCouncilPlayerStrategy(DatabaseContext databaseContext, UserManager<DbUser> userManager,
        DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _userManager = userManager;
        _discordService = discordService;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is not DecisionCouncilPlayer info) throw new InvalidOperationException();
        DbPlayer target =
            await _databaseContext.Players
                .Include(x => x.Identity)
                .FirstOrDefaultAsync(x => x.Id == info.PlayerId).ConfigureAwait(false)
            ?? throw new ArgumentOutOfRangeException(nameof(doc));
        string? oldPlayerPropertyValue = null!;
        switch (info)
        {
            case DecisionCouncilPlayerChange playerChangeInfo:
                switch (playerChangeInfo.Property)
                {
                    case DecisionCouncilPlayerChange.PlayerChangeProperty.Unknown:
                        break;
                    case DecisionCouncilPlayerChange.PlayerChangeProperty.Nickname:
                        oldPlayerPropertyValue = target.Nickname;
                        target.Nickname = playerChangeInfo.NewValue ?? string.Empty;
                        break;
                    case DecisionCouncilPlayerChange.PlayerChangeProperty.RealName:
                        oldPlayerPropertyValue = target.RealName;
                        target.RealName = playerChangeInfo.NewValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case DecisionCouncilPlayerKick kickInfo:
                if (target is not DbClanPlayer clanPlayer) throw new InvalidOperationException();
                foreach (DbExpedition expedition in _databaseContext.Expeditions)
                {
                    expedition.Members.Remove(target);
                    if (expedition.AccountablePlayerId == kickInfo.PlayerId)
                        expedition.AccountablePlayerId = kickInfo.SubstitutePlayerId;
                }

                oldPlayerPropertyValue = clanPlayer.ToString();
                target = new DbFormerClanPlayer
                {
                    Id = clanPlayer.Id,
                    DetectionDate = clanPlayer.DetectionDate,
                    JoinDate = clanPlayer.JoinDate,
                    Relationship = kickInfo.PlayerLeaveReason switch
                    {
                        PlayerLeaveReason.Unknown => PlayerRelationship.Unknown,
                        PlayerLeaveReason.AtWill => PlayerRelationship.Unknown,
                        PlayerLeaveReason.Suspend => PlayerRelationship.Unknown,
                        PlayerLeaveReason.Exclude => PlayerRelationship.Enemy,
                        PlayerLeaveReason.Exile => PlayerRelationship.Enemy,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    SteamId = clanPlayer.SteamId,
                    TimeZone = clanPlayer.TimeZone,
                    Status = PlayerStatus.Former,
                    LeaveDate = DateTime.UtcNow,
                    LeaveReason = kickInfo.PlayerLeaveReason,
                    Nickname = clanPlayer.Nickname,
                    RealName = clanPlayer.RealName,
                    RestorationAvailable =
                        kickInfo.PlayerLeaveReason is PlayerLeaveReason.AtWill or PlayerLeaveReason.Suspend,
                    DiscordId = clanPlayer.DiscordId
                };
                if (target.Identity is not null)
                {
                    target.Identity.LockoutEnabled = true;
                    target.Identity.LockoutEnd = null;
                    await _userManager.RemoveFromRolesAsync(target.Identity,
                        await _userManager.GetRolesAsync(target.Identity).ConfigureAwait(false)).ConfigureAwait(false);
                }

                _databaseContext.Update(target).State = EntityState.Modified;
                break;
            case not null:

                switch (info.Action)
                {
                    case DecisionCouncilPlayer.PlayerAction.Generic:
                        break;
                    case DecisionCouncilPlayer.PlayerAction.FromReserve:
                    case DecisionCouncilPlayer.PlayerAction.ToReserve:
                        if (target is not DbClanPlayer reservist) throw new InvalidOperationException();
                        reservist.OnReserve = info.Action == DecisionCouncilPlayer.PlayerAction.ToReserve;
                        break;
                    case DecisionCouncilPlayer.PlayerAction.Rehabilitation:
                        if (target is not DbFormerClanPlayer former) throw new InvalidOperationException();
                        former.RestorationAvailable = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(info));
        }

        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        switch (info)
        {
            case DecisionCouncilPlayerChange playerChangeInfo:
                switch (playerChangeInfo.Property)
                {
                    case DecisionCouncilPlayerChange.PlayerChangeProperty.Unknown:
                        break;
                    case DecisionCouncilPlayerChange.PlayerChangeProperty.Nickname:
                        await _discordService.SendBotLogMessageAsync(
                            $"Игрок <@{target.DiscordId:D}> сменил никнейм с «{oldPlayerPropertyValue}» на {target}"
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case DecisionCouncilPlayerChange.PlayerChangeProperty.RealName:
                        if (target.RealName is null)
                            await _discordService.SendBotLogMessageAsync(
                                $"Игрок <@{target.DiscordId:D}> скрыл упоминание своего имени"
                                + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        else
                            await _discordService.SendBotLogMessageAsync(
                                $"Игрок <@{target.DiscordId:D}> теперь предпочитает имя {target.RealName} вместо «{oldPlayerPropertyValue}»"
                                + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case DecisionCouncilPlayerKick kickInfo:
                switch (kickInfo.PlayerLeaveReason)
                {
                    case PlayerLeaveReason.Unknown:
                        await _discordService.SendBotLogMessageAsync(
                            $"Участие в клане игрока <@{target.DiscordId:D}>, известного для нас как «{oldPlayerPropertyValue}» было прекращено."
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case PlayerLeaveReason.AtWill:
                        await _discordService.SendBotLogMessageAsync(
                            $"Участие в клане игрока <@{target.DiscordId:D}>, известного для нас как «{oldPlayerPropertyValue}» было приостановлено согласно желанию игрока."
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case PlayerLeaveReason.Suspend:
                        await _discordService.SendBotLogMessageAsync(
                            $"Участие в клане игрока <@{target.DiscordId:D}>, известный для нас как «{oldPlayerPropertyValue}» было приостановлено. Надеемся на возвращение!.."
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case PlayerLeaveReason.Exclude:
                        await _discordService.SendBotLogMessageAsync(
                            $"Участие в клане игрока <@{target.DiscordId:D}>, известного для нас как «{oldPlayerPropertyValue}» было прекращено."
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case PlayerLeaveReason.Exile:
                        await _discordService.SendBotLogMessageAsync(
                            $"Участие в клане игрока <@{target.DiscordId:D}>, известного для нас как «{oldPlayerPropertyValue}» было прекращено, а он сам(а) объявлен(а) **врагом клана**!"
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case not null:

                switch (info.Action)
                {
                    case DecisionCouncilPlayer.PlayerAction.Generic:
                        break;
                    case DecisionCouncilPlayer.PlayerAction.FromReserve:
                        await _discordService.SendBotLogMessageAsync(
                            $"Игрок <@{target.DiscordId:D}> восстановлен(а) из резерва"
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case DecisionCouncilPlayer.PlayerAction.ToReserve:
                        await _discordService.SendBotLogMessageAsync(
                            $"Игрок <@{target.DiscordId:D}> переведен(а) в резерв"
                            + $"{Environment.NewLine}{Environment.NewLine}<https://andromeda-se.xyz/document/{doc.Id:D}>");
                        break;
                    case DecisionCouncilPlayer.PlayerAction.Rehabilitation:
                        break;
                }

                break;
        }
    }
}