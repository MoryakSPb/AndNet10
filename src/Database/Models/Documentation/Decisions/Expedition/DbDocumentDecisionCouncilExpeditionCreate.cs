using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;

public record DbDocumentDecisionCouncilExpeditionCreate : DbDocumentDecisionCouncilExpedition
{
    public override string Prefix { get; set; } = "РСЭС";
    public int AccountablePlayerId { get; set; }
    public DbPlayer AccountablePlayer { get; set; } = null!;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    public IList<DbPlayer> Members { get; set; } = Array.Empty<DbPlayer>();

    protected override async Task ExecuteAsync(DbClanPlayer executor, DatabaseContext context)
    {
        await base.ExecuteAsync(executor, context).ConfigureAwait(false);
        await context.Expeditions.AddAsync(new()
        {
            AccountablePlayer = AccountablePlayer,
            AccountablePlayerId = AccountablePlayerId,
            DiscordRoleId = null,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.Add(Duration),
            IsMarkedForDelete = false,
            Members = Members.ToList()
        }).ConfigureAwait(false);
    }
}