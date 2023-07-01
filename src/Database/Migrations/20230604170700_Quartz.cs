using AndNet.Manager.Database.Properties;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace AndNet.Manager.Database.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("20230604170700_Quartz")]
public partial class RegistryWorkerQuartz : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema("AndNet");
        migrationBuilder.Sql(Resources.tables_postgres_up);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(Resources.tables_postgres_down);
    }
}