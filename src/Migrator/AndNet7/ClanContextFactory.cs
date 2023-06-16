using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AndNet.Migrator.AndNet7.AndNet7;

public class ClanContextFactory : IDesignTimeDbContextFactory<ClanContext>
{
    public ClanContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ClanContext> builder = new();
        string connectionString = args.Any()
            ? string.Join(' ', args)
            : "User ID=postgres;Password=postgres;Host=localhost;Port=5432;";
        Console.WriteLine(@"connectionString = " + connectionString);
        return new(builder
            .UseNpgsql(connectionString)
            .Options);
    }
}