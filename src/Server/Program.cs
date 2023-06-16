using AndNet.Integration.Discord.Options;
using AndNet.Integration.Discord.Services;
using AndNet.Integration.Steam;
using AndNet.Integration.Steam.Options;
using AndNet.Manager.Database;
using AndNet.Manager.Database.Models.Auth;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SteamOptions>(builder.Configuration.GetRequiredSection("Steam"));
builder.Services.Configure<DiscordOptions>(builder.Configuration.GetRequiredSection("Discord"));
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
    .AddSteam()
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Discord:SecretKey"]!;
    });

builder.Services.AddHostedService<DiscordService>();
builder.Services.AddScoped<SteamClient>();
builder.Services.AddHttpClient();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("api/health");
app.MapFallbackToFile("index.html");

await app.RunAsync().ConfigureAwait(false);