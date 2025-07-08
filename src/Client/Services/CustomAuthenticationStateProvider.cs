using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Client.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenService _tokenService;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var isTokenValid = await _tokenService.IsTokenValidAsync();
            
            if (isTokenValid)
            {
                var claimsPrincipal = await _tokenService.GetClaimsPrincipalAsync();
                if (claimsPrincipal != null)
                {
                    _currentUser = claimsPrincipal;
                    return new AuthenticationState(_currentUser);
                }
            }

            // Token is invalid or doesn't exist
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(_currentUser);
        }
        catch
        {
            // In case of any error, return unauthenticated state
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(_currentUser);
        }
    }

    public async Task NotifyAuthenticationStateChangedAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public void MarkUserAsAuthenticated(ClaimsPrincipal user)
    {
        _currentUser = user;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public void MarkUserAsLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }
}
