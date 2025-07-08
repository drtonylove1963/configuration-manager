using Application.DTOs.Auth;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Services;

public interface IAuthenticationApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(LogoutRequest request);
    Task<UserInfo?> GetUserInfoAsync();
}

public class AuthenticationApiClient : IAuthenticationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1.0/auth/login", request, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1.0/auth/refresh", request, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1.0/auth/logout", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1.0/auth/user");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserInfo>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
