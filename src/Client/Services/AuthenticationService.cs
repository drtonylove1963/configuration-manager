using Application.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Client.Services;

public interface IAuthenticationService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> RefreshTokenAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    event Action<bool> AuthenticationStateChanged;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationApiClient _authApiClient;
    private readonly ITokenService _tokenService;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public event Action<bool> AuthenticationStateChanged = delegate { };

    public AuthenticationService(
        IAuthenticationApiClient authApiClient,
        ITokenService tokenService,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _authApiClient = authApiClient;
        _tokenService = tokenService;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var response = await _authApiClient.LoginAsync(loginRequest);
            
            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                await _tokenService.SetTokensAsync(response.AccessToken, response.RefreshToken);
                
                // Notify authentication state provider
                await _authStateProvider.NotifyAuthenticationStateChangedAsync();
                
                AuthenticationStateChanged.Invoke(true);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var logoutRequest = new LogoutRequest
                {
                    RefreshToken = refreshToken
                };
                
                await _authApiClient.LogoutAsync(logoutRequest);
            }
        }
        catch
        {
            // Continue with logout even if API call fails
        }
        finally
        {
            await _tokenService.ClearTokensAsync();

            // Notify authentication state provider
            await _authStateProvider.NotifyAuthenticationStateChangedAsync();

            AuthenticationStateChanged.Invoke(false);
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var refreshRequest = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };

            var response = await _authApiClient.RefreshTokenAsync(refreshRequest);
            
            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                await _tokenService.SetTokensAsync(response.AccessToken, response.RefreshToken);
                return true;
            }

            // If refresh fails, clear tokens
            await _tokenService.ClearTokensAsync();
            return false;
        }
        catch
        {
            await _tokenService.ClearTokensAsync();
            return false;
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        try
        {
            if (!await IsAuthenticatedAsync())
                return null;

            return await _authApiClient.GetUserInfoAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var isValid = await _tokenService.IsTokenValidAsync();
        
        if (!isValid)
        {
            // Try to refresh token
            isValid = await RefreshTokenAsync();
        }
        
        return isValid;
    }
}
