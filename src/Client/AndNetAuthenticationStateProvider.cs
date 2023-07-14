using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AndNet.Manager.Shared.Utility;
using Microsoft.AspNetCore.Components.Authorization;

namespace AndNet.Manager.Client;

public class AndNetAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState _emptyState = new(new(Array.Empty<ClaimsIdentity>()));
    private static AuthenticationState _authenticationState = _emptyState;
    private readonly HttpClient _httpClient;

    public AndNetAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task LoadIdentity()
    {
        using HttpResponseMessage answer = await _httpClient.GetAsync("api/auth/claims");
        ClaimRecord[] claims = await answer.Content.ReadFromJsonAsync<ClaimRecord[]>()
                               ?? throw new InvalidOperationException();
        _authenticationState = new(new(new ClaimsIdentity(
            claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer, x.OriginalIssuer)),
            nameof(Cookie),
            ClaimTypes.Name,
            ClaimTypes.Role)));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        using HttpRequestMessage request = new(HttpMethod.Head, "api/auth/claims");
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            if (ReferenceEquals(_authenticationState, _emptyState)) await LoadIdentity();
        }
        else
        {
            _authenticationState = _emptyState;
        }

        return _authenticationState;
    }
}