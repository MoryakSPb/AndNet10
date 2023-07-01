using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs.Election;

public class ElectionToResultsAnnounceJob : IJob
{
    private readonly DatabaseContext _databaseContext;

    public ElectionToResultsAnnounceJob(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DbElection election = await _databaseContext.Elections
                                  .FirstOrDefaultAsync(x => x.Stage == ElectionStage.Voting).ConfigureAwait(false)
                              ?? throw new InvalidOperationException();

        foreach (DbElectionCandidate candidate in _databaseContext.ElectionsCandidates
                     .Where(x => x.ElectionId == election.Id && x.Rating >= 0)
                     .OrderByDescending(x => x.Rating)
                     .ThenBy(x => x.RegistrationDate)
                     .Take(election.CouncilCapacity))
            candidate.IsWinner = true;

        election.Stage = ElectionStage.ResultsAnnounce;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
    }
}