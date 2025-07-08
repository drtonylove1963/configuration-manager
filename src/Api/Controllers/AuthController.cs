using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with tokens and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request, cancellationToken);
            
            if (!response.Success)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.Username);
                return Unauthorized(response);
            }

            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New login response with refreshed tokens</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RefreshTokenAsync(request, cancellationToken);
            
            if (!response.Success)
            {
                _logger.LogWarning("Token refresh failed");
                return Unauthorized(response);
            }

            _logger.LogInformation("Token refreshed successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    /// <param name="request">Logout request with refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.LogoutAsync(request, cancellationToken);
            
            if (!success)
            {
                _logger.LogWarning("Logout failed for user: {UserId}", GetCurrentUserId());
                return BadRequest(new { message = "Logout failed" });
            }

            _logger.LogInformation("User {UserId} logged out successfully", GetCurrentUserId());
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", GetCurrentUserId());
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user information</returns>
    [HttpGet("user-info")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userInfo = await _authService.GetCurrentUserAsync(userId, cancellationToken);

            if (userInfo == null)
            {
                _logger.LogWarning("User info not found for user: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info for user: {UserId}", GetCurrentUserId());
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate current access token
    /// </summary>
    /// <returns>Token validation status</returns>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public IActionResult ValidateToken()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            
            return Ok(new 
            { 
                valid = true, 
                userId = userId,
                userName = userName,
                message = "Token is valid" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return HandleException(ex);
        }
    }
}
