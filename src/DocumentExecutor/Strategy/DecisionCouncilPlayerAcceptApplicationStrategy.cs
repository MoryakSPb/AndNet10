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

public class DecisionCouncilPlayerAcceptApplicationStrategy : DocStrategy
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;
    private readonly UserManager<DbUser> _userManager;

    public DecisionCouncilPlayerAcceptApplicationStrategy(DatabaseContext databaseContext,
        UserManager<DbUser> userManager, DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _userManager = userManager;
        _discordService = discordService;
    }

    public override async Task Execute(DbDoc doc, DbClanPlayer executor)
    {
        if (doc.Info is not DecisionCouncilPlayerAcceptApplication info) throw new InvalidOperationException();
        DbPlayer target =
            await _databaseContext.Players.Include(x => x.Identity).FirstOrDefaultAsync(x => x.Id == info.PlayerId)
                .ConfigureAwait(false)
            ?? throw new ArgumentOutOfRangeException(nameof(doc));
        IQueryable<DbAward> awards = _databaseContext.Awards.Where(x => x.PlayerId == target.Id);
        DbClanPlayer newPlayer = new()
        {
            Rank = PlayerRank.Neophyte,
            DiscordId = target.DiscordId,
            DetectionDate = target.DetectionDate,
            JoinDate = DateTime.UtcNow,
            Nickname = target.Nickname,
            Status = PlayerStatus.Member,
            OnReserve = false,
            RealName = target.RealName,
            SteamId = target.SteamId,
            Id = target.Id,
            IdentityId = target.IdentityId,
            Identity = target.Identity,
            Awards = await awards.ToListAsync().ConfigureAwait(false)
        };

        newPlayer.Identity ??= new()
        {
            Player = target,
            UserName = target.Nickname,
            ConcurrencyStamp = Guid.NewGuid().ToString("n"),
            SecurityStamp = Guid.NewGuid().ToString("n")
        };
        newPlayer.Identity.LockoutEnabled = false;
        newPlayer.Identity.LockoutEnd = null;
        await _userManager.AddToRoleAsync(newPlayer.Identity, "member").ConfigureAwait(false);

        newPlayer.CalcPlayer();
        _databaseContext.Update(newPlayer).State = EntityState.Modified;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        await _discordService.UpdateGuildNicknameAsync(newPlayer.DiscordId!.Value, newPlayer.ToString())
            .ConfigureAwait(false);
        await _discordService.UpdateGuildRolesAsync(newPlayer.DiscordId!.Value, (false, true, false))
            .ConfigureAwait(false);
        await _discordService.SendBotLogMessageAsync($"Игрок <@{target.DiscordId:D}> присоединяется к нашему клану!"
                                                     + $"{Environment.NewLine}{Environment.NewLine}https://andromeda-se.xyz/document/{doc.Id:D}");
    }
}