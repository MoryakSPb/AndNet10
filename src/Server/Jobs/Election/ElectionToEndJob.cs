using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs.Election;

public class ElectionToEndJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;
    private readonly UserManager<DbUser> _userManager;

    public ElectionToEndJob(DatabaseContext databaseContext, UserManager<DbUser> userManager,
        DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _userManager = userManager;
        _discordService = discordService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DbElection election = await _databaseContext.Elections
                                  .FirstOrDefaultAsync(x => x.Stage == ElectionStage.ResultsAnnounce)
                                  .ConfigureAwait(false)
                              ?? throw new InvalidOperationException();
        List<DbClanPlayer> updatedPlayers = new();
        DbClanPlayer firstAdvisor = await _databaseContext.ClanPlayers
            .AsNoTrackingWithIdentityResolution()
            .FirstAsync(x => x.Rank == PlayerRank.FirstAdvisor).ConfigureAwait(false);
        foreach (DbClanPlayer clanPlayer in _databaseContext.ClanPlayers
                     .Include(x => x.Identity)
                     .Include(x => x.Awards)
                     .Include(x => x.Expeditions)
                     .Where(x => x.Rank == PlayerRank.Advisor))
        {
            clanPlayer.Rank = PlayerRank.Neophyte;
            clanPlayer.CalcPlayer();

            DbDoc doc = new()
            {
                AuthorId = firstAdvisor.Id,
                Author = firstAdvisor,
                CreationDate = DateTime.UtcNow,
                Info = new DecisionCouncilPlayerAwardSheet
                {
                    PlayerId = clanPlayer.Id,
                    AwardType = AwardType.Sapphire,
                    MinYesVotesPercent = AwardRules.MinCouncilVotes[AwardType.Sapphire],
                    Description =
                        $"Успешное выполнение обязанностей советника в период с {election.ElectionEnd.AddDays(-90):d} по {election.ElectionEnd:d}",
                    Action = DecisionCouncilPlayer.PlayerAction.Generic
                },
                Body = new()
                {
                    Body = @"# Наградной лист

Ввиду того, что советники не получают наград при большинстве условий, 
нам необходимо компенсировать неизбежные потери в их счёте, если считаем, 
что наш товарищ успешно справился(ась) с полномочиями советника."
                }
            };
            doc.GenerateTitleFromBody();
            await _databaseContext.Documents.AddAsync(doc).ConfigureAwait(false);

            if (clanPlayer.Identity is not null)
                await _userManager.RemoveFromRoleAsync(clanPlayer.Identity, "advisor").ConfigureAwait(false);

            updatedPlayers.Add(clanPlayer);
        }

        foreach (DbElectionCandidate winner in _databaseContext.ElectionsCandidates
                     .Include(x => x.Player)
                     .AsNoTracking()
                     .Where(x => x.ElectionId == election.Id && x.IsWinner))
            if (winner.Player is DbClanPlayer clanPlayer)
            {
                clanPlayer.Rank = PlayerRank.Advisor;
                if (clanPlayer.Identity is not null)
                    await _userManager.AddToRoleAsync(clanPlayer.Identity, "advisor").ConfigureAwait(false);

                updatedPlayers.Add(clanPlayer);
            }

        election.Stage = ElectionStage.Ended;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        await Task.WhenAll(updatedPlayers.Distinct().Where(x => x.DiscordId is not null).Select(async x =>
        {
            await _discordService.UpdateGuildNicknameAsync(x.DiscordId!.Value, x.ToString()).ConfigureAwait(false);
            await _discordService.UpdateGuildRolesAsync(x.DiscordId!.Value,
                    (x.Rank >= PlayerRank.Advisor, true, x.OnReserve),
                    x.Expeditions.Where(y => y.DiscordRoleId is not null).Select(y => y.DiscordRoleId!.Value))
                .ConfigureAwait(false);
        })).ConfigureAwait(false);
        await _discordService.SendBotLogMessageAsync("Выборы завершены! Избранные советники вступили в обязанности."
                                                     + Environment.NewLine + "https://andromeda-se.xyz/elections");
    }
}