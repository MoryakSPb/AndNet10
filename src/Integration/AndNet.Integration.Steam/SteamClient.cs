using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Web;
using AndNet.Integration.Steam.Models;
using AndNet.Integration.Steam.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AndNet.Integration.Steam;

public class SteamClient
{
    private const string _STEAM_VANITY_URL = "https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/";
    private const string _STEAM_ACTIVITY_URL = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/";
    private readonly HttpClient _httpClient;
    private readonly ILogger<SteamClient> _logger;
    private readonly IOptions<SteamOptions> _steamOptions;

    public SteamClient(HttpClient httpClient, ILogger<SteamClient> logger, IOptions<SteamOptions> steamOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _steamOptions = steamOptions;
    }

    public async IAsyncEnumerable<PlayerActivityResultNode> PlayersActivityAsync(IEnumerable<ulong> steamIds,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IEnumerable<Task<SteamResult<PlayerActivityResultCollection>?>> tasks = steamIds
            .Chunk(100)
            .Select(async x => await _httpClient.GetFromJsonAsync<SteamResult<PlayerActivityResultCollection>>(
                    _STEAM_ACTIVITY_URL
                    + $"?key={_steamOptions.Value.ApiKey}"
                    + $"&steamids={string.Join(',', x.Select(y => y.ToString("D")))}", cancellationToken)
                .ConfigureAwait(false));
        foreach (Task<SteamResult<PlayerActivityResultCollection>?> task in tasks)
        {
            SteamResult<PlayerActivityResultCollection>? chunk = await task.ConfigureAwait(false);
            if (chunk?.Result?.Players is null) continue;
            foreach (PlayerActivityResultNode player in chunk.Result.Players) yield return player;
        }
    }

    public async ValueTask<ulong?> ResolveSteamUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        url = url
            .Replace("http://", string.Empty)
            .Replace("https://", string.Empty)
            .Replace("www.", string.Empty)
            .Replace("steamcommunity.com/openid/id", string.Empty)
            .Replace("steamcommunity.com/id/", string.Empty)
            .Replace("steamcommunity.com/profiles/", string.Empty)
            .Trim('/');
        if (ulong.TryParse(url, out ulong result)) return result;

        SteamResult<ResolveSteamUrlResult>? answer = await _httpClient
            .GetFromJsonAsync<SteamResult<ResolveSteamUrlResult>>(
                $"{_STEAM_VANITY_URL}?key={_steamOptions.Value.ApiKey}&vanityurl={HttpUtility.UrlEncode(url.Trim())}",
                cancellationToken)
            .ConfigureAwait(false);
        return answer?.Result?.SteamId;
    }
}