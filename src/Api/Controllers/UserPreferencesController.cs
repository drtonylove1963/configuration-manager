using Application.DTOs.Configuration;
using Application.Interfaces;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0")]
public class UserPreferencesController : BaseApiController
{
    private readonly IConfigurationService _configurationService;
    private readonly IEnvironmentService _environmentService;
    private readonly ILogger<UserPreferencesController> _logger;
    
    private const string USER_PREFERENCES_KEY_PREFIX = "UserPreferences";
    private const string ENVIRONMENT_NAME = "UserPreferences";

    public UserPreferencesController(
        IConfigurationService configurationService,
        IEnvironmentService environmentService,
        ILogger<UserPreferencesController> logger)
    {
        _configurationService = configurationService;
        _environmentService = environmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get user's theme preference
    /// </summary>
    [HttpGet("theme")]
    public async Task<ActionResult<bool>> GetThemePreference()
    {
        try
        {
            var username = GetCurrentUserName();
            var themeValue = await GetUserPreferenceAsync(username, "ThemeMode");
            
            if (string.IsNullOrEmpty(themeValue))
            {
                return Ok(false); // Default to light mode
            }
            
            var isDarkMode = themeValue.Equals("dark", StringComparison.OrdinalIgnoreCase);
            return Ok(isDarkMode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme preference for user");
            return Ok(false); // Default to light mode on error
        }
    }

    /// <summary>
    /// Set user's theme preference
    /// </summary>
    [HttpPost("theme")]
    public async Task<ActionResult> SetThemePreference([FromBody] bool isDarkMode)
    {
        try
        {
            var username = GetCurrentUserName();
            var themeValue = isDarkMode ? "dark" : "light";
            
            await SetUserPreferenceAsync(username, "ThemeMode", themeValue, "User theme preference");
            
            _logger.LogInformation("Theme preference updated for user {Username}: {ThemeMode}", username, themeValue);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme preference for user");
            return StatusCode(500, "Error setting theme preference");
        }
    }

    /// <summary>
    /// Get a specific user preference
    /// </summary>
    [HttpGet("{key}")]
    public async Task<ActionResult<string>> GetPreference(string key)
    {
        try
        {
            var username = GetCurrentUserName();
            var value = await GetUserPreferenceAsync(username, key);
            
            if (value == null)
            {
                return NotFound($"Preference '{key}' not found for user");
            }
            
            return Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving preference {Key} for user", key);
            return StatusCode(500, "Error retrieving preference");
        }
    }

    /// <summary>
    /// Set a user preference
    /// </summary>
    [HttpPost("{key}")]
    public async Task<ActionResult> SetPreference(string key, [FromBody] string value)
    {
        try
        {
            var username = GetCurrentUserName();
            await SetUserPreferenceAsync(username, key, value, $"User preference: {key}");
            
            _logger.LogInformation("Preference {Key} updated for user {Username}", key, username);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting preference {Key} for user", key);
            return StatusCode(500, "Error setting preference");
        }
    }

    /// <summary>
    /// Get all user preferences
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, string>>> GetAllPreferences()
    {
        try
        {
            var username = GetCurrentUserName();
            var environmentId = await GetUserPreferencesEnvironmentIdAsync();
            var userKeyPrefix = BuildUserPreferenceKey(username, "");
            
            var allConfigs = await _configurationService.GetByEnvironmentAsync(environmentId);
            
            var userPreferences = allConfigs
                .Where(c => c.Key.StartsWith(userKeyPrefix))
                .ToDictionary(
                    c => ExtractPreferenceKeyFromConfigKey(c.Key, username),
                    c => c.Value
                );
            
            return Ok(userPreferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all preferences for user");
            return StatusCode(500, "Error retrieving preferences");
        }
    }

    private async Task<string?> GetUserPreferenceAsync(string username, string key)
    {
        var environmentId = await GetUserPreferencesEnvironmentIdAsync();
        var configKey = BuildUserPreferenceKey(username, key);
        
        var configuration = await _configurationService.GetByKeyAndEnvironmentAsync(configKey, environmentId);
        return configuration?.Value;
    }

    private async Task SetUserPreferenceAsync(string username, string key, string value, string description)
    {
        var environmentId = await GetUserPreferencesEnvironmentIdAsync();
        var configKey = BuildUserPreferenceKey(username, key);
        
        // Check if preference already exists
        var existingConfig = await _configurationService.GetByKeyAndEnvironmentAsync(configKey, environmentId);
        
        if (existingConfig != null)
        {
            // Update existing preference
            var updateDto = new UpdateConfigurationDto(
                value,
                ConfigurationValueType.String,
                description
            );

            await _configurationService.UpdateAsync(existingConfig.Id, updateDto, username);
        }
        else
        {
            // Create new preference
            var createDto = new CreateConfigurationDto(
                configKey,
                value,
                ConfigurationValueType.String,
                description,
                Guid.NewGuid(), // TODO: Get actual ApplicationId for user preferences
                environmentId
            );

            await _configurationService.CreateAsync(createDto, username);
        }
    }

    private async Task<Guid> GetUserPreferencesEnvironmentIdAsync()
    {
        // Try to get existing UserPreferences environment
        var environments = await _environmentService.GetAllAsync();
        var userPrefEnv = environments.FirstOrDefault(e => e.Name == ENVIRONMENT_NAME);
        
        if (userPrefEnv != null)
        {
            return userPrefEnv.Id;
        }
        
        // Create UserPreferences environment if it doesn't exist
        var createEnvDto = new Application.DTOs.Environment.CreateEnvironmentDto(
            ENVIRONMENT_NAME,
            "Environment for storing user preferences and application settings",
            999 // Put it at the end
        );

        var createdEnv = await _environmentService.CreateAsync(createEnvDto, "system");
        _logger.LogInformation("Created UserPreferences environment with ID: {EnvironmentId}", createdEnv.Id);
        return createdEnv.Id;
    }

    private static string BuildUserPreferenceKey(string username, string preferenceKey)
    {
        return $"{USER_PREFERENCES_KEY_PREFIX}:{username}:{preferenceKey}";
    }

    private static string ExtractPreferenceKeyFromConfigKey(string configKey, string username)
    {
        var prefix = $"{USER_PREFERENCES_KEY_PREFIX}:{username}:";
        return configKey.StartsWith(prefix) ? configKey.Substring(prefix.Length) : configKey;
    }
}
