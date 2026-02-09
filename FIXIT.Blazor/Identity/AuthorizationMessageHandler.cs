using System.Net;
using System.Net.Http.Headers;

namespace FIXIT.Blazor.Identity;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthenticationStateProvider _authProvider;
    private bool _isRefreshing = false;

    public AuthorizationMessageHandler(
        ILocalStorageService localStorage,
        CustomAuthenticationStateProvider authProvider)
    {
        _localStorage = localStorage;
        _authProvider = authProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // ⭐ إضافة التوكن للـ Request
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // إرسال الطلب
        var response = await base.SendAsync(request, cancellationToken);

        // ⭐ في حالة 401 - محاولة Refresh
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            _isRefreshing = true;

            try
            {
                var refreshed = await _authProvider.TryRefreshTokenAsync();

                if (refreshed)
                {
                    // إعادة المحاولة بالتوكن الجديد
                    var newToken = await _localStorage.GetItemAsync<string>("authToken");
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", newToken);

                    response = await base.SendAsync(request, cancellationToken);
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        return response;
    }
}
