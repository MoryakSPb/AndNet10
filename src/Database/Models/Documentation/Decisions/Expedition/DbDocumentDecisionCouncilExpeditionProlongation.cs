using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpeditionProlongation : DbDocumentDecisionCouncilExpedition
{
    public override string Prefix { get; set; } = "РСЭП";
    public TimeSpan ProlongationTime { get; set; }

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        Expedition.EndDate = Expedition.EndDate.Add(ProlongationTime);
    }
}