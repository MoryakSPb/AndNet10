using AndNet.Migrator.AndNet7.AndNet7.Shared;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Elections;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Programs;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AndNet.Migrator.AndNet7.AndNet7;

public class ClanContext : DbContext
{
    static ClanContext()
    {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanAwardTypeEnum>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanDepartmentEnum>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanElectionsStageEnum>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanMemberRankEnum>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanDruzhinaPositionEnum>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<DiscordChannelTypeEnum>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<DiscordPermissionsFlags>();
    }

    public ClanContext(DbContextOptions<ClanContext> options) : base(options)
    {
    }

    public DbSet<ClanElections> Elections { get; set; }
    public DbSet<ClanDruzhina> Druzhinas { get; set; }
    public DbSet<ClanMember> Members { get; set; }
    public DbSet<ClanProgram> Programs { get; set; }

    public DbSet<DiscordChannelCategory> ChannelCategories { get; set; }
    public DbSet<DiscordChannelMetadata> Channels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<ClanAwardTypeEnum>();
        modelBuilder.HasPostgresEnum<ClanDepartmentEnum>();
        modelBuilder.HasPostgresEnum<ClanElectionsStageEnum>();
        modelBuilder.HasPostgresEnum<ClanMemberRankEnum>();
        modelBuilder.HasPostgresEnum<ClanDruzhinaPositionEnum>();
        modelBuilder.HasPostgresEnum<DiscordChannelTypeEnum>();
        modelBuilder.HasPostgresEnum<DiscordPermissionsFlags>();

        modelBuilder.Entity<ClanElections>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AdvisorsStartDate).HasColumnType("date");
            entity.Property(x => x.Stage).IsRequired();
            entity.HasMany(x => x.Voting).WithOne(x => x.Elections);
        });

        modelBuilder.Entity<ClanElectionsVoting>(entity =>
        {
            entity.HasOne(x => x.Elections).WithMany(x => x.Voting).HasForeignKey(x => x.ElectionsId).IsRequired();
            entity.Property(x => x.Department);
            entity.Property(x => x.AgainstAll);
            entity.Ignore(x => x.VotesCount);
            entity.HasKey(x => new
            {
                x.ElectionsId,
                x.Department
            });
            entity.HasMany(x => x.Results).WithOne(x => x.Voting);
        });


        modelBuilder.Entity<ClanElectionsMember>(entity =>
        {
            entity.HasOne(x => x.Voting).WithMany(x => x.Results).HasForeignKey(x => new
            {
                x.ElectionsId,
                x.Department
            }).IsRequired();
            entity.HasOne(x => x.Member).WithMany(x => x.VoteMember).HasForeignKey(x => x.MemberId).IsRequired();
            entity.HasKey(x => new
            {
                x.ElectionsId,
                x.Department,
                x.MemberId
            });
            entity.Property(x => x.Votes);
            entity.Property(x => x.VoterId);
            entity.Property(x => x.Voted);
        });

        modelBuilder.Entity<ClanAward>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type);
            entity.HasOne(x => x.Member).WithMany(x => x.Awards).IsRequired();
            entity.Property(x => x.Date).HasColumnType("date").IsRequired();
            entity.Property(x => x.Description).IsRequired();
        });

        modelBuilder.Entity<ClanDruzhina>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique(false);
            entity.Property(x => x.Department).IsRequired();
            entity.Ignore(x => x.ActiveMembers);
            entity.HasMany(x => x.MembersHistory).WithOne(x => x.Druzhina);
            entity.Property(x => x.CreationDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.DisbandDate).HasColumnType("date").IsRequired(false);
        });

        modelBuilder.Entity<ClanDruzhinaMember>(entity =>
        {
            entity.HasOne(x => x.Druzhina).WithMany(x => x.MembersHistory).HasForeignKey(x => x.DruzhinaId)
                .IsRequired();
            entity.HasOne(x => x.Member).WithMany(x => x.AllDruzhinasMember).HasForeignKey(x => x.MemberId)
                .IsRequired();
            entity.HasKey(x => new
            {
                x.DruzhinaId,
                x.MemberId
            });
            entity.Property(x => x.JoinDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.LeaveDate).HasColumnType("date");
            entity.Property(x => x.Position);
        });

        modelBuilder.Entity<ClanMember>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasAlternateKey(x => x.SteamId);
            entity.HasAlternateKey(x => x.DiscordId);
            entity.HasIndex(x => x.DiscordId).IsUnique();
            entity.Property(x => x.VkId);
            entity.Property(x => x.TelegramId);
            entity.HasAlternateKey(x => x.Nickname);
            entity.HasIndex(x => x.Nickname).IsUnique();
            entity.Property(x => x.RealName);
            entity.Property(x => x.JoinDate).HasColumnType("date");
            entity.Property(x => x.Rank);
            entity.Property(x => x.Department);
            entity.HasMany(x => x.Programs).WithMany(x => x.Members);
            entity.HasMany(x => x.Awards).WithOne(x => x.Member);
            entity.HasMany(x => x.VoteMember).WithOne(x => x.Member);
            entity.HasMany(x => x.AllDruzhinasMember).WithOne(x => x.Member);
        });

        modelBuilder.Entity<ClanProgram>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique(false);
            entity.HasMany(x => x.Tasks).WithOne(x => x.Program);
            entity.HasOne(x => x.Initiator);
            entity.Property(x => x.CreationDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.DisbandDate).HasColumnType("date").IsRequired();
        });

        modelBuilder.Entity<ClanProgramTask>(entity =>
        {
            entity.HasOne(x => x.Program).WithMany(x => x.Tasks).HasForeignKey(x => x.ProgramId).IsRequired();
            entity.Property(x => x.TaskNumber).IsRequired();
            entity.HasKey(x => new
            {
                x.ProgramId,
                x.TaskNumber
            });
            entity.Property(x => x.Status);
            entity.Property(x => x.TaskDescription).IsRequired();
            entity.Property(x => x.FinalDescription);
        });

        modelBuilder.Entity<DiscordChannelMetadata>(entity =>
        {
            entity.Property(x => x.DiscordId).ValueGeneratedNever();
            entity.HasKey(x => x.DiscordId);
            entity.Property(x => x.Name);
            entity.HasIndex(x => x.Name).IsUnique(false);
            entity.Property(x => x.ChannelPosition);
            entity.Property(x => x.CategoryPosition);
            entity.HasOne(x => x.Category).WithMany(x => x.Channels).HasForeignKey(x => x.CategoryPosition)
                .IsRequired(false);
            entity.Property(x => x.Type);
        });

        modelBuilder.Entity<DiscordChannelCategory>(entity =>
        {
            entity.Property(x => x.DiscordId).ValueGeneratedNever();
            entity.Property(x => x.Position).ValueGeneratedNever();
            entity.HasKey(x => x.Position);
            entity.Property(x => x.Name);
            entity.HasIndex(x => x.Name).IsUnique(false);
            entity.HasMany(x => x.Channels).WithOne(x => x.Category).HasForeignKey(x => x.CategoryPosition);
        });

        modelBuilder.Entity<DiscordDepartmentPermissions>(entity =>
        {
            entity.Property(x => x.ChannelId).ValueGeneratedNever();
            entity.Property(x => x.Department);
            entity.HasKey(x => new
            {
                x.ChannelId,
                x.Department
            });
            entity.HasOne(x => x.Metadata).WithMany(x => x.DepartmentsPermissions).HasForeignKey(x => x.ChannelId);
            entity.Property(x => x.Permissions);
        });
    }
}