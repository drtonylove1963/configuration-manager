using Application.DTOs.Auth;
using Application.Settings;
using Application.Interfaces;
using Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IApplicationUserRepository applicationUserRepository,
        IApplicationRepository applicationRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _applicationUserRepository = applicationUserRepository;
        _applicationRepository = applicationRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by username or email
            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken) ??
                      await _userRepository.GetByEmailAsync(request.Username, cancellationToken);

            if (user == null || !user.IsActive)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password",
                    Errors = new List<string> { "Authentication failed" }
                };
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password",
                    Errors = new List<string> { "Authentication failed" }
                };
            }

            // Get user's application access
            var applicationAccess = await _applicationUserRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
            
            // If application key is provided, validate access to that specific application
            string? selectedApplicationId = null;
            if (!string.IsNullOrEmpty(request.ApplicationKey))
            {
                var application = await _applicationRepository.GetByApplicationKeyAsync(request.ApplicationKey, cancellationToken);
                if (application == null || !applicationAccess.Any(aa => aa.ApplicationId == application.Id))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Access denied to the specified application",
                        Errors = new List<string> { "Application access denied" }
                    };
                }
                selectedApplicationId = application.Id.ToString();
            }

            // Get user roles (global roles from UserRoles)
            var globalRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            
            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(
                user.Id.ToString(),
                user.Username,
                user.Email,
                globalRoles,
                selectedApplicationId);

            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.Add(_jwtSettings.RefreshTokenExpiry);

            // Update user with refresh token and last login
            user.SetRefreshToken(refreshToken, refreshTokenExpiry);
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Create user info
            var userInfo = new UserInfo
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = globalRoles,
                Applications = applicationAccess.Select(aa => new ApplicationAccess
                {
                    ApplicationId = aa.ApplicationId.ToString(),
                    ApplicationName = aa.Application.Name,
                    ApplicationKey = aa.Application.ApplicationKey,
                    Role = aa.Role.Name,
                    IsActive = aa.IsActive,
                    LastAccessedAt = aa.LastAccessedAt
                }).ToList(),
                IsActive = user.IsActive,
                IsEmailConfirmed = user.IsEmailConfirmed,
                LastLoginAt = user.LastLoginAt
            };

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.Add(_jwtSettings.AccessTokenExpiry),
                RefreshTokenExpiry = refreshTokenExpiry,
                User = userInfo,
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user ID from the access token (even if expired)
            var userId = _tokenService.GetUserIdFromToken(request.AccessToken);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid access token",
                    Errors = new List<string> { "Token validation failed" }
                };
            }

            // Validate refresh token
            if (!await ValidateRefreshTokenAsync(userId, request.RefreshToken, cancellationToken))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid refresh token",
                    Errors = new List<string> { "Refresh token validation failed" }
                };
            }

            // Get user and generate new tokens
            var user = await _userRepository.GetByIdAsync(userGuid, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found or inactive",
                    Errors = new List<string> { "User validation failed" }
                };
            }

            // Get user's application access and roles
            var applicationAccess = await _applicationUserRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
            var globalRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(
                user.Id.ToString(),
                user.Username,
                user.Email,
                globalRoles);

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.Add(_jwtSettings.RefreshTokenExpiry);

            // Update user with new refresh token
            user.SetRefreshToken(newRefreshToken, refreshTokenExpiry);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return new LoginResponse
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.Add(_jwtSettings.AccessTokenExpiry),
                RefreshTokenExpiry = refreshTokenExpiry,
                Message = "Token refreshed successfully"
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during token refresh",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<bool> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by refresh token
            var users = await _userRepository.GetAllAsync(cancellationToken);
            var user = users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            if (user != null)
            {
                user.ClearRefreshToken();
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return null;

        var user = await _userRepository.GetByIdAsync(userGuid, cancellationToken);
        if (user == null || !user.IsActive)
            return null;

        var applicationAccess = await _applicationUserRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        var globalRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        return new UserInfo
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = globalRoles,
            Applications = applicationAccess.Select(aa => new ApplicationAccess
            {
                ApplicationId = aa.ApplicationId.ToString(),
                ApplicationName = aa.Application.Name,
                ApplicationKey = aa.Application.ApplicationKey,
                Role = aa.Role.Name,
                IsActive = aa.IsActive,
                LastAccessedAt = aa.LastAccessedAt
            }).ToList(),
            IsActive = user.IsActive,
            IsEmailConfirmed = user.IsEmailConfirmed,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return false;

        var user = await _userRepository.GetByIdAsync(userGuid, cancellationToken);
        if (user == null || !user.IsActive)
            return false;

        return user.RefreshToken == refreshToken && 
               user.RefreshTokenExpiryTime.HasValue && 
               user.RefreshTokenExpiryTime.Value > DateTime.UtcNow;
    }
}
