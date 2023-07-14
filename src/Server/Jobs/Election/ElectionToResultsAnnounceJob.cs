using System.Text;
using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs.Election;

public class ElectionToResultsAnnounceJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;

    public ElectionToResultsAnnounceJob(DatabaseContext databaseContext, DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _discordService = discordService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DbElection election = await _databaseContext.Elections
                                  .Include(x => x.ElectionCandidates)
                                  .ThenInclude(x => x.Player)
                                  .FirstOrDefaultAsync(x => x.Stage == ElectionStage.Voting).ConfigureAwait(false)
                              ?? throw new InvalidOperationException();

        foreach (DbElectionCandidate candidate in election.ElectionCandidates
                     .Where(x => x.Rating >= 0)
                     .OrderByDescending(x => x.Rating)
                     .ThenBy(x => x.RegistrationDate)
                     .Take(election.CouncilCapacity))
            candidate.IsWinner = true;

        election.Stage = ElectionStage.ResultsAnnounce;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        StringBuilder table = new();
        int nicknameMax = Math.Max(8, election.ElectionCandidates.Max(x => x.Player.Nickname.Length));
        table.AppendLine("```md");
        table.AppendLine($"| {"Участник".PadRight(nicknameMax)} | * | 123 |");
        table.AppendLine($"| {string.Empty.PadRight(nicknameMax, '-')} | - | --- |");
        foreach (DbElectionCandidate candidate in election.ElectionCandidates.OrderByDescending(x => x.IsWinner)
                     .ThenByDescending(x => x.Rating).ThenBy(x => x.RegistrationDate))
            table.AppendLine(
                $"| {candidate.Player.Nickname.PadRight(nicknameMax)} | {(candidate.IsWinner ? "*" : " ")} | {candidate.Rating.ToString("+00;-00")} |");
        table.AppendLine("```");

        await _discordService.SendBotLogMessageAsync("Голосование на выборах завершено!" + Environment.NewLine + table
                                                     + Environment.NewLine + "https://andromeda-se.xyz/elections");
    }
}