using System.Text.Json;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.Database.Models.Election;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace AndNet.Manager.Database;

public class DatabaseContext : IdentityDbContext<DbUser, IdentityRole<int>, int>
{
    private static readonly JsonSerializerOptions _serializerOptions = JsonSerializerOptions.Default;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<DbAward> Awards { get; set; } = null!;

    public DbSet<DbPlayer> Players { get; set; } = null!;
    public DbSet<DbClanPlayer> ClanPlayers { get; set; } = null!;
    public DbSet<DbExternalPlayer> ExternalPlayers { get; set; } = null!;
    public DbSet<DbFormerClanPlayer> FormerClanMembers { get; set; } = null!;
    public DbSet<DbExpedition> Expeditions { get; set; } = null!;
    public DbSet<DbDoc> Documents { get; set; } = null!;
    public DbSet<DbDocBody> DocumentBodies { get; set; } = null!;
    public DbSet<DbElection> Elections { get; set; } = null!;
    public DbSet<DbElectionCandidate> ElectionsCandidates { get; set; } = null!;
    public DbSet<DbElectionVoter> ElectionsVoters { get; set; } = null!;
    public DbSet<DbPlayerStat> PlayerStats { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("AndNet");
        builder.HasPostgresExtension("AndNet", "rum");

        builder.HasDbFunction(typeof(Extensions).GetMethod(nameof(Extensions.Distance),
                new[] { typeof(NpgsqlTsVector), typeof(NpgsqlTsQuery) }) ?? throw new ArgumentNullException(),
            functionBuilder =>
            {
                functionBuilder.HasTranslation(x =>
                    new PostgresUnknownBinaryExpression(x[0], x[1], "<=>", typeof(float),
                        new FloatTypeMapping("float4")));
            });

        OnModelCreatingBase(builder);
        OnModelCreatingDocumentation(builder);

        base.OnModelCreating(builder);
        OnModelCreatingIdentity(builder);
        OnModelCreatingElection(builder);

        builder.Entity<DbPlayerStat>(entity =>
        {
            entity.HasKey(x => new { x.PlayerId, x.Date });

            entity.HasIndex(x => x.PlayerId).HasMethod("Hash");
            entity.HasIndex(x => x.Date).HasMethod("BRIN");

            entity.Property(x => x.Status);

            entity.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId);

            entity.Property(x => x.Version).IsRowVersion();
        });
    }

    private static void OnModelCreatingDocumentation(ModelBuilder builder)
    {
        builder.Entity<DbDocBody>(entity =>
        {
            entity.HasKey(x => x.DocId);
            entity.HasOne(x => x.Doc).WithOne(x => x.Body).HasForeignKey<DbDocBody>(x => x.DocId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Body).UseCompressionMethod("pglz");

            entity.Property(x => x.SearchVector).ValueGeneratedOnAddOrUpdate();
            entity.HasGeneratedTsVectorColumn(x => x.SearchVector, "russian", x => x.Body);
            entity.HasIndex(x => x.SearchVector).HasMethod("RUM");

            entity.Property(x => x.Version).IsRowVersion();
        });

        builder.Entity<DbDoc>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title);

            entity.Property(x => x.CreationDate).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.CreationDate).IsUnique(false).HasMethod("BTree");

            entity.HasOne(x => x.Author).WithMany(x => x.CreatedDocuments).HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.AuthorId).IsUnique(false).HasMethod("Hash");

            entity.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ParentId).IsUnique(false).HasMethod("Hash");

            entity.HasOne(x => x.Body).WithOne(x => x.Doc).HasForeignKey<DbDocBody>(x => x.DocId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Views);

            entity.Property(x => x.Info).HasColumnType("jsonb").HasConversion(
                x => JsonSerializer.Serialize(x, _serializerOptions),
                x => JsonSerializer.Deserialize<DocInfo>(x, _serializerOptions),
                ValueComparer.CreateDefault(typeof(DocInfo), true)).HasColumnName(nameof(DbDoc.Info));
            entity.HasIndex(x => x.Info).HasMethod("GIN").HasOperators("jsonb_path_ops");

            entity.Ignore(x => x.ChildIds);

            entity.Property(x => x.Version).IsRowVersion();
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

            entity.Property(x => x.IssueDate).HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.AwardSheet).WithOne().HasForeignKey<DbAward>(x => x.AwardSheetId)
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

            entity.Property(x => x.During).HasColumnType("tstzrange");
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

            entity.ToTable("PlayerContacts");
        });

        builder.Entity<DbPlayer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasDiscriminator(x => x.Status)
                .HasValue<DbClanPlayer>(PlayerStatus.Member)
                .HasValue<DbExternalPlayer>(PlayerStatus.External)
                .HasValue<DbFormerClanPlayer>(PlayerStatus.Former);

            entity.Property(x => x.Nickname);
            entity.HasIndex(x => x.Nickname).IsUnique().HasMethod("Btree");

            entity.Property(x => x.TimeZone).HasConversion(x => x == null ? null : x.Id,
                id => id == null ? null : TimeZoneInfo.FindSystemTimeZoneById(id));
            entity.HasIndex(x => x.TimeZone).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.RealName);

            entity.Property(x => x.SteamId);
            entity.HasIndex(x => x.SteamId).IsUnique().HasMethod("Btree");

            entity.Property(x => x.DiscordId);
            entity.HasIndex(x => x.DiscordId).IsUnique().HasMethod("Btree");

            entity.Property(x => x.DetectionDate).HasColumnType("timestamp with time zone");

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

            entity.Property(x => x.JoinDate).HasColumnType("timestamp with time zone")
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

            entity.Property(x => x.JoinDate).HasColumnType("timestamp with time zone")
                .HasColumnName(nameof(DbClanPlayer.JoinDate));

            entity.Property(x => x.LeaveDate).HasColumnType("timestamp with time zone");

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

    private static void OnModelCreatingElection(ModelBuilder builder)
    {
        builder.Entity<DbElectionVoter>(entity =>
        {
            entity.HasKey(x => new { x.ElectionId, x.PlayerId });

            entity.HasOne(x => x.Election).WithMany(x => x.Voters).HasForeignKey(x => x.ElectionId);
            entity.HasIndex(x => x.ElectionId).IsUnique(false).IsDescending().HasMethod("Hash");
            entity.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.VoteDate);

            entity.Property(x => x.Version).IsRowVersion();
        });

        builder.Entity<DbElectionCandidate>(entity =>
        {
            entity.HasKey(x => new { x.ElectionId, x.PlayerId });
            entity.Property(x => x.ElectionId).HasColumnOrder(0);
            entity.Property(x => x.PlayerId).HasColumnOrder(1);

            entity.HasOne(x => x.Election).WithMany(x => x.ElectionCandidates).HasForeignKey(x => x.ElectionId);
            entity.HasIndex(x => x.ElectionId).IsUnique(false).IsDescending().HasMethod("Hash");
            entity.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId);
            entity.HasIndex(x => x.PlayerId).IsUnique(false).HasMethod("Hash");

            entity.Property(x => x.RegistrationDate);
            entity.Property(x => x.Rating);
            entity.Property(x => x.IsWinner);

            entity.Property(x => x.Version).IsRowVersion();
        });

        builder.Entity<DbElection>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Stage);
            entity.HasIndex(x => x.Stage).HasMethod("Hash");

            entity.Property(x => x.ElectionEnd);

            entity.Property(x => x.CouncilCapacity);

            entity.Ignore(x => x.AllVotersCount);
            entity.Ignore(x => x.VotedVotersCount);
            entity.Ignore(x => x.Candidates);

            entity.HasMany(x => x.ElectionCandidates).WithOne(x => x.Election).HasForeignKey(x => x.ElectionId);
            entity.HasMany(x => x.Voters).WithOne(x => x.Election).HasForeignKey(x => x.ElectionId);

            entity.Property(x => x.Version).IsRowVersion();

            entity.HasData(new DbElection
            {
                Id = 1,
                Stage = ElectionStage.NotStarted,
                ElectionEnd = new DateTime(2023, 8, 1).ToLocalTime().ToUniversalTime()
            });
        });
    }
}