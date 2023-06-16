using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Shared.Utility;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client;

public class AndNetAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public AndNetAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        HttpResponseMessage answer = await _httpClient.GetAsync("api/auth/claims");
        if (answer.IsSuccessStatusCode)
        {
            ClaimRecord[] claims = await answer.Content.ReadFromJsonAsync<ClaimRecord[]>()
                                   ?? throw new InvalidOperationException();
            return new(new(new ClaimsIdentity(
                claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer, x.OriginalIssuer)),
                nameof(Cookie),
                ClaimTypes.Name,
                ClaimTypes.Role)));
        }

        return new(new(Array.Empty<ClaimsIdentity>()));
    }
}