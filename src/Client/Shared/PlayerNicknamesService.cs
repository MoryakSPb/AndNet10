using System.Collections.Immutable;

namespace AndNet.Manager.Client.Shared;

public sealed class PlayerNicknamesService
{
    private static readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(30);

    private readonly HttpClient _httpClient;

    public PlayerNicknamesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static ImmutableSortedDictionary<int, (DateTime create, string nickname)> Cache { get; set; }
        = ImmutableSortedDictionary<int, (DateTime create, string nickname)>.Empty;

    public async Task LoadNicknames(IEnumerable<int> ids)
    {
        await Task.WhenAll(ids.Select(async x => await GetFullNicknameAsync(x)));
    }

    public async ValueTask<string> GetFullNicknameAsync(int id)
    {
        if (Cache.TryGetValue(id, out (DateTime create, string nickname) tuple)
            && tuple.create.Add(_cacheLifetime) >= DateTime.UtcNow)
            return tuple.nickname;

        string nickname = await _httpClient.GetStringAsync($"api/Player/{id}/nickname");
        Cache = Cache.SetItem(id, (DateTime.UtcNow, nickname));
        return nickname;
    }

    public static string GetFullNicknameFromCache(int id)
    {
        return Cache[id].nickname;
    }
}