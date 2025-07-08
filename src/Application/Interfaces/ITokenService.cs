using System.Security.Claims;

namespace Application.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="username">The user's username</param>
    /// <param name="email">The user's email address</param>
    /// <param name="roles">The user's roles</param>
    /// <param name="applicationId">The current application context (optional)</param>
    /// <returns>The JWT access token</returns>
    string GenerateAccessToken(string userId, string username, string email, IEnumerable<string> roles, string? applicationId = null);
    
    /// <summary>
    /// Generates a secure refresh token
    /// </summary>
    /// <returns>A cryptographically secure refresh token</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>The claims principal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Extracts the user ID from a JWT token
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The user ID if found, null otherwise</returns>
    string? GetUserIdFromToken(string token);
    
    /// <summary>
    /// Extracts the username from a JWT token
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The username if found, null otherwise</returns>
    string? GetUsernameFromToken(string token);
    
    /// <summary>
    /// Checks if a JWT token is expired
    /// </summary>
    /// <param name="token">The JWT token to check</param>
    /// <returns>True if the token is expired, false otherwise</returns>
    bool IsTokenExpired(string token);
    
    /// <summary>
    /// Gets the expiration time of a JWT token
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The expiration time if found, null otherwise</returns>
    DateTime? GetTokenExpiration(string token);
}
