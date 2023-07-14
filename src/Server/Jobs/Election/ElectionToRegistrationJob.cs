using AndNet.Integration.Discord.Services;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AndNet.Manager.Server.Jobs.Election;

public class ElectionToRegistrationJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly DiscordService _discordService;

    public ElectionToRegistrationJob(DatabaseContext databaseContext, DiscordService discordService)
    {
        _databaseContext = databaseContext;
        _discordService = discordService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DbElection election = await _databaseContext.Elections
                                  .FirstOrDefaultAsync(x => x.Stage == ElectionStage.NotStarted).ConfigureAwait(false)
                              ?? throw new InvalidOperationException();
        election.Stage = ElectionStage.Registration;
        await _databaseContext.SaveChangesAsync().ConfigureAwait(false);
        await _discordService.SendBotLogMessageAsync("Регистрация на выборы открыта!" + Environment.NewLine
            + "https://andromeda-se.xyz/elections");
    }
}