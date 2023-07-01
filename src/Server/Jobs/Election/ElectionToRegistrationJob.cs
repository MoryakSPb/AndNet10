using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs.Election;

public class ElectionToRegistrationJob : IJob
{
    private readonly DatabaseContext _databaseContext;

    public ElectionToRegistrationJob(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DbElection election = await _databaseContext.Elections
                                  .FirstOrDefaultAsync(x => x.Stage == ElectionStage.NotStarted).ConfigureAwait(false)
                              ?? throw new InvalidOperationException();
        election.Stage = ElectionStage.Registration;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
    }
}