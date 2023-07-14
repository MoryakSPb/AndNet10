using AndNet.Integration.Discord.Options;
using AndNet.Integration.Discord.Services;
using AndNet.Integration.Steam;
using AndNet.Integration.Steam.Options;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Auth;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Server.Jobs;
using AndNet.Manager.Server.Jobs.Election;
using AndNet.Manager.Server.Options;
using AndNet.Manager.Server.Utility;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#if DEBUG
DiscordService.SilentMode = true;
#endif

builder.Services.Configure<SteamOptions>(builder.Configuration.GetRequiredSection("Steam"));
builder.Services.Configure<DiscordOptions>(builder.Configuration.GetRequiredSection("Discord"));
builder.Services.Configure<AwardOptions>(builder.Configuration.GetRequiredSection("Award"));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = context => context.CookieOptions.SameSite = SameSiteMode.Unspecified;
    options.OnDeleteCookie = context => context.CookieOptions.SameSite = SameSiteMode.Unspecified;
});

builder.Services.AddSingleton<DiscordService>();
builder.Services.AddHostedService(x => x.GetRequiredService<DiscordService>());
builder.Services.AddSingleton<SteamClient>();

builder.Services.AddDbContextPool<DatabaseContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
#if DEBUG
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
#endif

builder.Services.AddIdentity<DbUser, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.User.RequireUniqueEmail = false;
        //TODO: options.User.AllowedUserNameCharacters
    })
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddSteam(options => options.CorrelationCookie.SameSite = SameSiteMode.Unspecified)
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Discord:SecretKey"]!;
    });

builder.Services.AddHostedService<DiscordService>();
builder.Services.AddScoped<SteamClient>();
builder.Services.AddHttpClient();
builder.Services.AddDocumentService();

builder.Services.AddTransient<CalcPlayersJob>();
builder.Services.AddTransient<ActivityStatsJob>();

builder.Services.AddQuartz(ConfigureQuartz);
builder.Services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

builder.Services.AddResponseCaching();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

builder.Services.AddHealthChecks()
    .AddApplicationStatus("manager-application-status")
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres-status")
    .AddUrlGroup(new Uri("https://api.steampowered.com/ISteamWebAPIUtil/GetServerInfo/v1/"),
        HttpMethod.Head,
        "steam-api-status")
    .AddUrlGroup(new Uri("https://discord.com/api/v10/sticker-packs"),
        HttpMethod.Head,
        "discord-api-status");

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseResponseCaching();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("api/health");
app.MapFallbackToFile("index.html");

await app.RunAsync().ConfigureAwait(false);

void ConfigureQuartz(IServiceCollectionQuartzConfigurator configurator)
{
    configurator.UseMicrosoftDependencyInjectionJobFactory();
    configurator.UseDefaultThreadPool(1);
    configurator.UseSimpleTypeLoader();
    configurator.UsePersistentStore(options =>
    {
        options.UseProperties = true;
        options.UseClustering();
        options.UsePostgres(builder.Configuration.GetConnectionString("Postgres")!);
        options.UseSerializer<TextJsonSerializer>();
    });
    configurator.Properties["quartz.jobStore.tablePrefix"] = "\"AndNet\".QRTZ_";

    configurator.ScheduleJob<CalcPlayersJob>(triggerConfigurator => triggerConfigurator
        .WithIdentity(nameof(CalcPlayersJob) + "Trigger")
        .StartNow()
        .WithDailyTimeIntervalSchedule(x => x.OnEveryDay()
            .WithIntervalInHours(1)
            .WithMisfireHandlingInstructionIgnoreMisfires()));

    configurator.ScheduleJob<ActivityStatsJob>(triggerConfigurator => triggerConfigurator
        .WithIdentity(nameof(ActivityStatsJob) + "Trigger")
        .StartNow()
        .WithDailyTimeIntervalSchedule(x => x.OnEveryDay()
            .WithIntervalInMinutes(15)
            .WithMisfireHandlingInstructionIgnoreMisfires()));

    DateTime firstEnd = DateTime.Parse(builder.Configuration.GetRequiredSection("Elections")["FirstEndDate"]
                                       ?? throw new InvalidOperationException());
    firstEnd = firstEnd.ToLocalTime().ToUniversalTime();

    configurator.ScheduleJob<ElectionToRegistrationJob>(triggerConfigurator => triggerConfigurator
        .WithIdentity(nameof(ElectionToRegistrationJob) + "Trigger")
        .WithPriority(1400)
        .StartAt(firstEnd.AddDays(-30))
        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(90))
            .RepeatForever()
            .WithMisfireHandlingInstructionFireNow()));

    configurator.ScheduleJob<ElectionToVotingJob>(triggerConfigurator => triggerConfigurator
        .WithIdentity(nameof(ElectionToVotingJob) + "Trigger")
        .WithPriority(1200)
        .StartAt(firstEnd.AddDays(-15))
        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(90))
            .RepeatForever()
            .WithMisfireHandlingInstructionFireNow()));

    configurator.ScheduleJob<ElectionToResultsAnnounceJob>(triggerConfigurator => triggerConfigurator
        .WithIdentity(nameof(ElectionToResultsAnnounceJob) + "Trigger")
        .WithPriority(1100)
        .StartAt(firstEnd.AddDays(-4))
        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(90))
            .RepeatForever()
            .WithMisfireHandlingInstructionFireNow()));

    configurator.ScheduleJob<ElectionToEndJob>(triggerConfigurator => triggerConfigurator
        .WithIdentity(nameof(ElectionToEndJob) + "Trigger")
        .WithPriority(1000)
        .StartAt(firstEnd)
        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(90))
            .RepeatForever()
            .WithMisfireHandlingInstructionFireNow()));
}