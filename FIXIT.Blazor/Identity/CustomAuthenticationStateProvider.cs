using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FIXIT.Blazor.Identity;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    public ILogger<CustomAuthenticationStateProvider> Logger { get; set; }

    private DateTime _lastRefreshCheck = DateTime.MinValue;
    private readonly TimeSpan _refreshCheckInterval = TimeSpan.FromMinutes(1);

    public CustomAuthenticationStateProvider(
        ILocalStorageService localStorage,
        HttpClient httpClient,
        NavigationManager navigationManager,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        Logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return CreateAnonymousState();
            }

            var claims = ParseClaimsFromJwt(token);

            if (IsTokenExpired(claims))
            {
                var refreshed = await TryRefreshTokenAsync();

                if (!refreshed)
                {
                    await ClearAuthenticationAsync();
                    return CreateAnonymousState();
                }

                token = await _localStorage.GetItemAsync<string>("authToken");
                claims = ParseClaimsFromJwt(token);
            }
            else if (ShouldCheckForRefresh() && IsTokenNearExpiry(claims))
            {
                _ = Task.Run(async () => await TryRefreshTokenAsync());
                _lastRefreshCheck = DateTime.UtcNow;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error while getting authentication state");
            return CreateAnonymousState();
        }
    }
    public async Task MarkUserAsAuthenticated(string accessToken, string refreshToken)
    {
        await _localStorage.SetItemAsync("authToken", accessToken);
        await _localStorage.SetItemAsync("refreshToken", refreshToken);

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }
    public async Task MarkUserAsLoggedOut()
    {
        await ClearAuthenticationAsync();
        NotifyAuthenticationStateChanged(
            Task.FromResult(CreateAnonymousState()));
    }
    public async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var response = await _httpClient.PostAsJsonAsync("Auth/RefreshToken",
                new { Token = refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                await ClearAuthenticationAsync();
                NotifyAuthenticationStateChanged(Task.FromResult(CreateAnonymousState())); 
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();

            if (result?.IsAuthenticated == true &&
                !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);

                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                // ⭐ إشعار بتغيير حالة Authentication
                var claims = ParseClaimsFromJwt(result.Token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(user)));

                return true;
            }

            return false;
        }
        catch
        {
            await ClearAuthenticationAsync();
            return false;
        }
    }
    private async Task ClearAuthenticationAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
    private AuthenticationState CreateAnonymousState()
    {
        return new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity()));
    }
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
    private bool IsTokenExpired(IEnumerable<Claim> claims)
    {
        var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expiry == null)
            return true;

        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry));
        return expiryDate <= DateTimeOffset.UtcNow;
    }

    private bool IsTokenNearExpiry(IEnumerable<Claim> claims)
    {
        var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expiry == null)
            return true;

        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry));
        var bufferTime = TimeSpan.FromMinutes(5);

        return expiryDate <= DateTimeOffset.UtcNow.Add(bufferTime);
    }
    private bool ShouldCheckForRefresh()
    {
        return DateTime.UtcNow - _lastRefreshCheck > _refreshCheckInterval;
    }

    private class RefreshTokenResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }
    }
}
