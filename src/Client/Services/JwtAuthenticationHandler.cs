using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Client.Services;

public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;

    public JwtAuthenticationHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the access token
        var accessToken = await _tokenService.GetAccessTokenAsync();
        
        // Add the token to the request if it exists
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        // Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // If we get a 401 Unauthorized, try to refresh the token
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Try to refresh the token
                var refreshSuccess = await TryRefreshTokenAsync(refreshToken);
                
                if (refreshSuccess)
                {
                    // Get the new access token and retry the request
                    var newAccessToken = await _tokenService.GetAccessTokenAsync();
                    
                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Create a new HTTP client for the refresh request to avoid circular dependency
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5021"); // Default API base URL

            var refreshRequest = new
            {
                RefreshToken = refreshToken
            };

            var response = await httpClient.PostAsJsonAsync("/api/v1.0/auth/refresh", refreshRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var refreshResponse = System.Text.Json.JsonSerializer.Deserialize<RefreshTokenResponse>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (refreshResponse != null && !string.IsNullOrEmpty(refreshResponse.AccessToken))
                {
                    await _tokenService.SetTokensAsync(refreshResponse.AccessToken, refreshResponse.RefreshToken);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
