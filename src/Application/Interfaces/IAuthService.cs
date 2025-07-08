using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    /// <param name="request">The login request containing credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with tokens and user information</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New login response with refreshed tokens</returns>
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="request">The logout request containing refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if logout was successful</returns>
    Task<bool> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets current user information from user ID
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if found</returns>
    Task<UserInfo?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates if a refresh token is valid and belongs to the user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="refreshToken">The refresh token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the refresh token is valid</returns>
    Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default);
}
