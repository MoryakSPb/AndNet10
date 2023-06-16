using System.Net;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Documentation;
using AndNet.Manager.Database.Models.Documentation.Decisions;
using AndNet.Manager.Database.Models.Documentation.Decisions.Directive;
using AndNet.Manager.Database.Models.Documentation.Decisions.Expedition;
using AndNet.Manager.Database.Models.Documentation.Decisions.Player;
using AndNet.Manager.Database.Models.Documentation.Decisions.Utility;
using AndNet.Manager.Database.Models.Documentation.Report;
using AndNet.Manager.Database.Models.Documentation.Report.Utility;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AndNet.Manager.Database;

public class DatabaseContext : IdentityDbContext<DbUser, IdentityRole<int>, int>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<DbAward> Awards { get; set; } = null!;

    public DbSet<DbPlayer> Players { get; set; } = null!;
    public DbSet<DbClanPlayer> ClanPlayers { get; set; } = null!;
    public DbSet<DbExternalPlayer> ExternalPlayers { get; set; } = null!;
    public DbSet<DbFormerClanPlayer> FormerClanMembers { get; set; } = null!;
    public DbSet<DbExpedition> Expeditions { get; set; } = null!;
    public DbSet<DbDocument> Documents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("AndNet");
        builder.HasPostgresExtension("AndNet", "rum");

        OnModelCreatingBase(builder);
        OnModelCreatingDocumentation(builder);

        base.OnModelCreating(builder);
        OnModelCreatingIdentity(builder);
    }

    private static void OnModelCreatingDocumentation(ModelBuilder builder)
    {
        builder.Entity<DbDocument>(entity =>
        {
            entity.UseTphMappingStrategy();
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreationDate).HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");
            entity.HasAlternateKey(x => x.CreationDate);

            entity.Property(x => x.Prefix).UseCollation("C");
            entity.HasIndex(x => x.Prefix).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.Body);

            entity.HasOne(x => x.Creator).WithMany(x => x.CreatedDocuments).HasForeignKey(x => x.CreatorId);
            entity.HasIndex(x => x.CreatorId).IsUnique(false).HasMethod("Hash");

            entity.HasOne(x => x.Parent).WithMany(x => x.ChildrenDocuments).HasForeignKey(x => x.ParentId);
            entity.HasIndex(x => x.ParentId).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.SearchVector).ValueGeneratedOnAddOrUpdate();
            entity.HasGeneratedTsVectorColumn(x => x.SearchVector, "russian", x => x.Body);
            entity.HasIndex(x => x.SearchVector).HasMethod("RUM");

            entity.Property(x => x.Version).IsRowVersion();

            entity.HasDiscriminator(x => x.Prefix)
                .HasValue<DbDocument>(string.Empty)
                .HasValue<DbDocumentDirective>("Д")
                .HasValue<DbDocumentProtocol>("П")
                .HasValue<DbDocumentReport>("О")
                .HasValue<DbDocumentReportBattle>("ОС")
                .HasValue<DbDocumentReportExpedition>("ОЭ")
                .HasValue<DbDocumentDecision>("Р")
                .HasValue<DbDocumentDecisionGeneralMeeting>("РО")
                .HasValue<DbDocumentDecisionCouncil>("РС")
                .HasValue<DbDocumentDecisionCouncilGeneralMeetingInit>("РСОС")
                .HasValue<DbDocumentDecisionCouncilDirective>("РСД")
                .HasValue<DbDocumentDecisionCouncilDirectiveAccept>("РСДП")
                .HasValue<DbDocumentDecisionCouncilDirectiveCancel>("РСДО")
                .HasValue<DbDocumentDecisionCouncilDirectiveChange>("РСДЗ")
                .HasValue<DbDocumentDecisionCouncilExpedition>("РСЭ")
                .HasValue<DbDocumentDecisionCouncilExpeditionAddPlayer>("РСЭД")
                .HasValue<DbDocumentDecisionCouncilExpeditionChangeAccountable>("РСЭК")
                .HasValue<DbDocumentDecisionCouncilExpeditionClose>("РСЭЗ")
                .HasValue<DbDocumentDecisionCouncilExpeditionCreate>("РСЭС")
                .HasValue<DbDocumentDecisionCouncilExpeditionProlongation>("РСЭП")
                .HasValue<DbDocumentDecisionCouncilExpeditionRemovePlayer>("РСЭИ")
                .HasValue<DbDocumentDecisionCouncilPlayer>("РСИ")
                .HasValue<DbDocumentDecisionCouncilPlayerAcceptApplication>("РСИО")
                .HasValue<DbDocumentDecisionCouncilPlayerAwardSheet>("РСИН")
                .HasValue<DbDocumentDecisionCouncilPlayerFromReserve>("РСИВ")
                .HasValue<DbDocumentDecisionCouncilPlayerKick>("РСИИ")
                .HasValue<DbDocumentDecisionCouncilPlayerRehabilitation>("РСИП")
                .HasValue<DbDocumentDecisionCouncilPlayerToReserve>("РСИР");
        });

        builder.Entity<DbDocumentProtocol>(entity =>
        {
            entity.Property(x => x.ProtocolType);
            entity.HasIndex(x => x.ProtocolType).IsUnique(false).HasMethod("Hash");

            entity.HasMany(x => x.Members).WithMany(x => x.RelatedProtocols);
        });

        builder.Entity<DbDocumentDirective>(entity =>
        {
            entity.Property(x => x.CancelDate).HasColumnType("timestamp without time zone");
            entity.HasIndex(x => x.CancelDate).IsUnique(false).IsDescending().HasMethod("BTree");

            entity.Property(x => x.AcceptanceDate).HasColumnType("timestamp without time zone");
            entity.HasIndex(x => x.AcceptanceDate).IsUnique(false).HasMethod("BTree");

            entity.HasOne(x => x.ReplacedBy).WithMany(x => x.Previous).HasForeignKey(x => x.ReplacedById);
            entity.HasIndex(x => x.ReplacedById).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentReport>(entity =>
        {
            entity.Ignore(x => x.StartDate);
            entity.Ignore(x => x.EndDate);

            entity.Property(x => x.ReportRange).HasColumnType("tsrange");
            entity.HasIndex(x => x.ReportRange).IsUnique(false).HasMethod("GiST");
        });

        builder.Entity<DbDocumentReportBattle>(entity =>
        {
            entity.Property(x => x.ServerEndPoint)
                .HasConversion(x => new ValueTuple<IPAddress, int>(x!.Address, x.Port), x => new(x.Item1, x.Item2))
                .HasColumnType("cidr");
            entity.HasIndex(x => x.ServerEndPoint).IsUnique(false).HasMethod("GiST").HasOperators("inet_ops");

            entity.HasMany(x => x.Combatants).WithOne(x => x.Battle).HasForeignKey(x => x.BattleId);
        });

        builder.Entity<DbDocumentReportExpedition>(entity =>
        {
            entity.Property(x => x.ExpeditionId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilExpedition.ExpeditionId));
            entity.HasOne(x => x.Expedition).WithMany(x => x.Reports).HasForeignKey(x => x.ExpeditionId);
            entity.HasIndex(x => x.ExpeditionId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbBattleCombatant>(entity =>
        {
            entity.HasKey(x => new { x.BattleId, x.Number });

            entity.HasOne(x => x.Battle).WithMany(x => x.Combatants).HasForeignKey(x => x.BattleId);
            entity.HasIndex(x => x.BattleId).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.Number);

            entity.Property(x => x.Tag);
            entity.HasIndex(x => x.Tag).IsUnique(false).HasMethod("BTree");

            entity.Property(x => x.Name);
            entity.HasIndex(x => x.Name).IsUnique(false).HasMethod("BTree");

            entity.HasOne(x => x.Commander).WithMany(x => x.BattleCombatantsCommander)
                .HasForeignKey(x => x.CommanderId);
            entity.HasIndex(x => x.CommanderId).IsUnique(false).HasMethod("Hash");

            entity.HasMany(x => x.Players).WithMany(x => x.BattleCombatantsMember);

            entity.Property(x => x.UnknownPlayers).HasPostgresArrayConversion(x => x, x => x);
            entity.HasIndex(x => x.UnknownPlayers).IsUnique(false).HasMethod("GIN");

            entity.Property(x => x.Units).HasPostgresArrayConversion(x => x, x => x);
            entity.HasIndex(x => x.Units).IsUnique(false).HasMethod("GIN");

            entity.Property(x => x.Casualties).HasPostgresArrayConversion(x => x, x => x);
            entity.HasIndex(x => x.Casualties).IsUnique(false).HasMethod("GIN");
        });

        builder.Entity<DbDocumentDecision>(entity =>
        {
            entity.HasMany(x => x.Votes).WithOne(x => x.Decision).HasForeignKey(x => x.DecisionId);

            entity.Ignore(x => x.ActualVotes);
            entity.Ignore(x => x.BlockingVotes);
            entity.Ignore(x => x.YesVotes);
            entity.Ignore(x => x.NoVotes);
            entity.Ignore(x => x.AgreePercent);
            entity.Ignore(x => x.IsAgreeAvailable);
            entity.Ignore(x => x.IsDeclineAvailable);

            entity.Property(x => x.IsExecuted);

            entity.HasOne(x => x.Executor).WithMany(x => x.ExecutedDecisions).HasForeignKey(x => x.ExecutorId);

            entity.Property(x => x.ExecuteDate).HasColumnType("timestamp without time zone");
            ;
            entity.HasIndex(x => x.ExecuteDate).IsUnique(false).HasMethod("BTree");
        });

        builder.Entity<DbVote>(entity =>
        {
            entity.HasKey(x => new { x.DecisionId, x.PlayerId });

            entity.HasOne(x => x.Decision).WithMany(x => x.Votes).HasForeignKey(x => x.DecisionId);
            entity.HasIndex(x => x.DecisionId).IsUnique(false).HasMethod("Hash");

            entity.HasOne(x => x.Player).WithMany(x => x.Votes).HasForeignKey(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.VoteType);
            entity.HasIndex(x => x.VoteType).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionGeneralMeeting>(_ => { });

        builder.Entity<DbDocumentDecisionCouncil>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilGeneralMeetingInit>(entity =>
        {
            entity.Property(x => x.Date).HasColumnType("timestamp without time zone");
            entity.HasIndex(x => x.Date).IsUnique().HasMethod("BTree");
        });

        builder.Entity<DbDocumentDecisionCouncilDirective>(entity =>
        {
            entity.Property(x => x.DirectiveId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilDirective.DirectiveId));
            entity.HasOne(x => x.Directive).WithMany(x => x.Directives).HasForeignKey(x => x.DirectiveId);
            entity.HasIndex(x => x.DirectiveId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilDirectiveAccept>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilDirectiveCancel>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilDirectiveChange>(entity =>
        {
            entity.Property(x => x.NewDirectiveId);
            entity.HasOne(x => x.NewDirective).WithMany(x => x.ChangeToDirectives).HasForeignKey(x => x.NewDirectiveId);
            entity.HasIndex(x => x.NewDirectiveId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilPlayer>(entity =>
        {
            entity.Property(x => x.PlayerId).HasColumnName(nameof(DbDocumentDecisionCouncilPlayer.PlayerId));
            entity.HasOne(x => x.Player).WithMany(x => x.CouncilPlayerDirectives).HasForeignKey(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilPlayerAcceptApplication>(entity =>
        {
            entity.Property(x => x.PlayerId).HasColumnName(nameof(DbDocumentDecisionCouncilPlayer.PlayerId));
            entity.Property(x => x.Recommendation);
            entity.Property(x => x.Hours);
            entity.Property(x => x.Age);
            entity.Property(x => x.TimeZone).HasConversion(x => x.Id, id => TimeZoneInfo.FindSystemTimeZoneById(id));
        });

        builder.Entity<DbDocumentDecisionCouncilPlayerAwardSheet>(entity =>
        {
            entity.Property(x => x.AwardType);
            entity.Property(x => x.Description);
        });

        builder.Entity<DbDocumentDecisionCouncilPlayerFromReserve>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilPlayerToReserve>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilPlayerRehabilitation>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilPlayerKick>(entity => { entity.Property(x => x.PlayerLeaveReason); });

        builder.Entity<DbDocumentDecisionCouncilExpedition>(entity =>
        {
            entity.Property(x => x.ExpeditionId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilExpedition.ExpeditionId));
            entity.HasOne(x => x.Expedition).WithMany(x => x.CouncilPlayerDirectives)
                .HasForeignKey(x => x.ExpeditionId);
            entity.HasIndex(x => x.ExpeditionId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilExpeditionAddPlayer>(entity =>
        {
            entity.Property(x => x.PlayerId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilPlayer.PlayerId));

            entity.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilExpeditionRemovePlayer>(entity =>
        {
            entity.Property(x => x.PlayerId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilPlayer.PlayerId));

            entity.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilExpeditionChangeAccountable>(entity =>
        {
            entity.Property(x => x.AccountablePlayerId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilPlayer.PlayerId));

            entity.HasOne(x => x.AccountablePlayer).WithMany().HasForeignKey(x => x.AccountablePlayerId);
            entity.HasIndex(x => x.AccountablePlayerId).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbDocumentDecisionCouncilExpeditionClose>(_ => { });

        builder.Entity<DbDocumentDecisionCouncilExpeditionCreate>(entity =>
        {
            entity.Property(x => x.AccountablePlayerId)
                .HasColumnName(nameof(DbDocumentDecisionCouncilExpeditionAddPlayer.PlayerId));

            entity.HasOne(x => x.AccountablePlayer).WithMany().HasForeignKey(x => x.AccountablePlayerId);
            entity.HasIndex(x => x.AccountablePlayerId).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.Duration).HasColumnName(nameof(DbDocumentDecisionCouncilExpeditionCreate.Duration));

            entity.HasMany(x => x.Members).WithMany();
        });

        builder.Entity<DbDocumentDecisionCouncilExpeditionProlongation>(entity =>
        {
            entity.Property(x => x.ProlongationTime)
                .HasColumnName(nameof(DbDocumentDecisionCouncilExpeditionCreate.Duration));
        });
    }

    private static void OnModelCreatingBase(ModelBuilder builder)
    {
        builder.Entity<DbAward>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");
            entity.HasOne(x => x.Player).WithMany(x => x.Awards)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.AwardType);
            entity.HasIndex(x => x.AwardType).IsUnique(false).HasMethod("Btree");

            entity.Property(x => x.IssuerId);
            entity.HasOne(x => x.Issuer).WithMany(x => x.IssuedAwards)
                .HasForeignKey(x => x.IssuerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.AutomationId);
            entity.HasIndex(x => x.AutomationId).IsUnique(false).HasMethod("Hash");
            entity.HasIndex(x => new { x.PlayerId, x.AutomationId }).IsUnique(false).HasMethod("BTree");

            entity.Property(x => x.Description);

            entity.Property(x => x.IssueDate).HasColumnType("timestamp without time zone");

            entity.HasOne(x => x.AwardSheet).WithOne(x => x.Award).HasForeignKey<DbAward>(x => x.AwardSheetId)
                .IsRequired();
            entity.HasIndex(x => x.AwardSheetId).IsUnique().HasMethod("Btree");

            entity.Property(x => x.Version).IsRowVersion();

            entity.Property(x => x.IsMarkedForDelete);
            entity.HasIndex(x => x.IsMarkedForDelete).IsUnique(false).HasMethod("Hash");
            entity.HasQueryFilter(x => !x.IsMarkedForDelete);
        });

        builder.Entity<DbExpedition>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Ignore(x => x.StartDate);
            entity.Ignore(x => x.EndDate);

            entity.Property(x => x.During).HasColumnType("tsrange");
            entity.HasIndex(x => x.During).IsUnique(false).HasMethod("GiST");

            entity.Property(x => x.AccountablePlayerId);
            entity.HasIndex(x => x.AccountablePlayerId).IsUnique(false).HasMethod("Hash");
            entity.HasOne(x => x.AccountablePlayer).WithMany(x => x.AccountableExpeditions)
                .HasForeignKey(x => x.AccountablePlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.DiscordRoleId);
            entity.HasIndex(x => x.DiscordRoleId).IsUnique().HasMethod("Btree");

            entity.HasMany(x => x.Members).WithMany(x => x.Expeditions).UsingEntity(typeBuilder =>
            {
                typeBuilder.HasIndex("MembersId").HasMethod("Hash");
                typeBuilder.HasIndex("ExpeditionsId").HasMethod("Hash");
                typeBuilder.ToTable("ExpeditionsPlayers");
            });

            entity.Property(x => x.Version).IsRowVersion();

            entity.Property(x => x.IsMarkedForDelete);
            entity.HasIndex(x => x.IsMarkedForDelete).IsUnique(false).HasMethod("Hash");
            entity.HasQueryFilter(x => !x.IsMarkedForDelete);
        });

        builder.Entity<DbPlayerContact>(entity =>
        {
            entity.HasKey(x => new { x.PlayerId, x.Type });

            entity.Property(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");
            entity.HasOne(x => x.Player).WithMany(x => x.Contacts)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Type);
            entity.HasIndex(x => x.Type).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.Value);

            entity.Property(x => x.Version).IsRowVersion();

            entity.Property(x => x.IsMarkedForDelete);
            entity.HasIndex(x => x.IsMarkedForDelete).IsUnique(false).HasMethod("Hash");
            entity.HasQueryFilter(x => !x.IsMarkedForDelete);
        });

        builder.Entity<DbPlayer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasDiscriminator(x => x.Status)
                .HasValue<DbClanPlayer>(PlayerStatus.Member)
                .HasValue<DbExternalPlayer>(PlayerStatus.External)
                .HasValue<DbFormerClanPlayer>(PlayerStatus.Former);

            entity.Property(x => x.Nickname);
            entity.HasAlternateKey(x => x.Nickname);

            entity.Property(x => x.TimeZone).HasConversion(x => x == null ? null : x.Id,
                id => id == null ? null : TimeZoneInfo.FindSystemTimeZoneById(id));
            entity.HasIndex(x => x.TimeZone).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.RealName);

            entity.Property(x => x.SteamId);
            entity.HasIndex(x => x.SteamId).IsUnique().HasMethod("Btree");

            entity.Property(x => x.DiscordId);
            entity.HasIndex(x => x.DiscordId).IsUnique().HasMethod("Btree");

            entity.Property(x => x.DetectionDate).HasColumnType("timestamp without time zone");

            entity.HasMany(x => x.Contacts).WithOne(x => x.Player)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Awards).WithOne(x => x.Player)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.IssuedAwards).WithOne(x => x.Issuer)
                .HasForeignKey(x => x.IssuerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.AccountableExpeditions).WithOne(x => x.AccountablePlayer)
                .HasForeignKey(x => x.AccountablePlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Expeditions).WithMany(x => x.Members).UsingEntity(typeBuilder =>
            {
                typeBuilder.HasIndex("MembersId").IsUnique(false).HasMethod("Hash");
                typeBuilder.HasIndex("ExpeditionsId").IsUnique(false).HasMethod("Hash");
                typeBuilder.ToTable("ExpeditionsPlayers");
            });

            entity.Property(x => x.IdentityId);
            entity.HasOne(x => x.Identity).WithOne(x => x.Player).HasForeignKey<DbPlayer>(x => x.IdentityId);
            entity.HasIndex(x => x.IdentityId).IsUnique().HasMethod("Btree");

            entity.Property(x => x.Version).IsRowVersion();
        });

        builder.Entity<DbClanPlayer>(entity =>
        {
            entity.HasBaseType<DbPlayer>();

            entity.Property(x => x.JoinDate).HasColumnType("timestamp without time zone")
                .HasColumnName(nameof(DbClanPlayer.JoinDate));

            entity.Property(x => x.Rank);
            entity.HasIndex(x => x.Rank).IsUnique(false).HasMethod("Btree");

            entity.Property(x => x.Score);

            entity.Property(x => x.OnReserve);
            entity.HasIndex(x => x.OnReserve).IsUnique(false).HasMethod("Hash");
        });

        builder.Entity<DbExternalPlayer>(entity =>
        {
            entity.HasBaseType<DbPlayer>();

            entity.Property(x => x.Relationship);
            entity.HasIndex(x => x.Relationship).IsUnique(false).HasMethod("Btree");
        });

        builder.Entity<DbFormerClanPlayer>(entity =>
        {
            entity.HasBaseType<DbExternalPlayer>();

            entity.Property(x => x.JoinDate).HasColumnType("timestamp without time zone")
                .HasColumnName(nameof(DbClanPlayer.JoinDate));

            entity.Property(x => x.LeaveDate).HasColumnType("timestamp without time zone");

            entity.Property(x => x.LeaveReason);

            entity.Property(x => x.RestorationAvailable);
        });
    }

    private static void OnModelCreatingIdentity(ModelBuilder builder)
    {
        builder.Entity<IdentityRole<int>>(entity =>
        {
            entity.HasData(new IdentityRole<int>
            {
                Id = -1,
                Name = "first_advisor",
                NormalizedName = "FIRST_ADVISOR",
                ConcurrencyStamp = "757dc6530e744053874ea3b66d54b90a"
            }, new IdentityRole<int>
            {
                Id = -2,
                Name = "advisor",
                NormalizedName = "ADVISOR",
                ConcurrencyStamp = "3a160416081c4744840bd246115d39b8"
            }, new IdentityRole<int>
            {
                Id = -3,
                Name = "member",
                NormalizedName = "MEMBER",
                ConcurrencyStamp = "b1debd2c3d2c4214a1ace0d9ddba5dff"
            });
        });
    }
}