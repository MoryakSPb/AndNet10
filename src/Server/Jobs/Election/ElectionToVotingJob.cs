using System.Collections.Immutable;
using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Shared;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs.Election;

public class ElectionToVotingJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;
    private readonly DocumentService _documentService;

    public ElectionToVotingJob(DatabaseContext databaseContext, DocumentService documentService,
        DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _documentService = documentService;
        _discordService = discordService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DbElection election = await _databaseContext.Elections
                                  .Include(x => x.Voters)
                                  .FirstOrDefaultAsync(x => x.Stage == ElectionStage.Registration).ConfigureAwait(false)
                              ?? throw new InvalidOperationException();

        await _databaseContext.ElectionsVoters.AddRangeAsync(await _databaseContext.ClanPlayers
            .Where(x => x.Rank >= PlayerRank.Neophyte)
            .ToAsyncEnumerable()
            .Select(x => new DbElectionVoter
            {
                PlayerId = x.Id,
                ElectionId = election.Id,
                VoteDate = null
            })
            .ToArrayAsync().ConfigureAwait(false)).ConfigureAwait(false);

        DbClanPlayer firstAdvisor = await _databaseContext.ClanPlayers
            .AsNoTrackingWithIdentityResolution()
            .FirstAsync(x => x.Rank == PlayerRank.FirstAdvisor).ConfigureAwait(false);
        List<DbDoc> awardSheets = new(8);
        foreach (DbPlayer player in _databaseContext.ElectionsCandidates.Include(x => x.Player)
                     .Where(x => x.ElectionId == election.Id)
                     .Select(x => x.Player))
        {
            if (player is not DbClanPlayer { Rank: < PlayerRank.Advisor }) continue;
            DbDoc doc = new()
            {
                Author = firstAdvisor,
                AuthorId = firstAdvisor.Id,
                CreationDate = DateTime.UtcNow,
                Info = new DecisionCouncilPlayerAwardSheet
                {
                    PlayerId = player.Id,
                    Votes = ImmutableList<Decision.Vote>.Empty,
                    MinYesVotesPercent = AwardRules.MinCouncilVotes[AwardType.Copper],
                    Action = DecisionCouncilPlayer.PlayerAction.Generic,
                    AwardType = AwardType.Silver,
                    Description = "Участие в выборах совета в качестве избираемого",
                    PredeterminedIssueDate = context.FireTimeUtc.UtcDateTime,
                    AutomationId = 4
                },
                Body = new()
                {
                    Body = @"# Наградной лист

Награда выдана автоматически."
                }
            };
            awardSheets.Add(doc);
            await _databaseContext.Documents.AddAsync(doc).ConfigureAwait(false);
        }

        election.Stage = ElectionStage.Voting;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (DbDoc awardSheet in awardSheets)
            await _documentService.AgreeExecuteAsync(awardSheet, firstAdvisor).ConfigureAwait(false);
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        await _discordService.SendBotLogMessageAsync("Голосование на выборах началось!" + Environment.NewLine
            + "https://andromeda-se.xyz/elections");
    }
}