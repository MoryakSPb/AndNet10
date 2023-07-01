using AndNet.Manager.Database;
using AndNet.Manager.DocumentExecutor;
using AndNet.Migrator.AndNet7;
using AndNet.Migrator.AndNet7.AndNet7;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDocumentService();

builder.Services.AddDbContext<ClanContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("OldPostgres")));
builder.Services.AddDbContext<DatabaseContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("NewPostgres")));

builder.Services.AddHostedService<Migrator>();

IHost host = builder.Build();

await host.RunAsync().ConfigureAwait(false);