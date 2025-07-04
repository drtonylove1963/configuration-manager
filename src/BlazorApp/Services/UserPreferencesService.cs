using BlazorApp.Models;
using Client.Services;
using Application.DTOs.Configuration;
using Domain.ValueObjects;

namespace BlazorApp.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfigurationApiClient _configurationApiClient;
    private readonly IEnvironmentApiClient _environmentApiClient;
    private readonly ILogger<UserPreferencesService> _logger;
    
    // Constants for user preference configuration keys
    private const string USER_PREFERENCES_KEY_PREFIX = "UserPreferences";
    private const string THEME_PREFERENCE_KEY = "ThemeMode";
    private const string ENVIRONMENT_NAME = "UserPreferences"; // Special environment for user preferences
    
    private Guid? _userPreferencesEnvironmentId;

    public UserPreferencesService(
        ICurrentUserService currentUserService,
        IConfigurationApiClient configurationApiClient,
        IEnvironmentApiClient environmentApiClient,
        ILogger<UserPreferencesService> logger)
    {
        _currentUserService = currentUserService;
        _configurationApiClient = configurationApiClient;
        _environmentApiClient = environmentApiClient;
        _logger = logger;
    }

    public async Task<bool> GetThemePreferenceAsync()
    {
        try
        {
            var themeValue = await GetPreferenceAsync(THEME_PREFERENCE_KEY);
            
            // Default to light mode if no preference is set
            if (string.IsNullOrEmpty(themeValue))
            {
                _logger.LogDebug("No theme preference found for user, defaulting to light mode");
                return false; // Light mode
            }
            
            var isDarkMode = themeValue.Equals("dark", StringComparison.OrdinalIgnoreCase);
            _logger.LogDebug("Retrieved theme preference for user: {ThemeMode}", isDarkMode ? "dark" : "light");
            return isDarkMode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving theme preference, defaulting to light mode");
            return false; // Default to light mode on error
        }
    }

    public async Task SetThemePreferenceAsync(bool isDarkMode)
    {
        try
        {
            var themeValue = isDarkMode ? "dark" : "light";
            await SetPreferenceAsync(THEME_PREFERENCE_KEY, themeValue);
            _logger.LogInformation("Theme preference updated for user: {ThemeMode}", themeValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting theme preference");
            throw;
        }
    }

    public async Task<string?> GetPreferenceAsync(string key)
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var environmentId = await GetUserPreferencesEnvironmentIdAsync();
            var configKey = BuildUserPreferenceKey(currentUser.Username, key);
            
            var configuration = await _configurationApiClient.GetByKeyAndEnvironmentAsync(configKey, environmentId);
            
            if (configuration != null)
            {
                _logger.LogDebug("Retrieved user preference {Key} = {Value}", key, configuration.Value);
                return configuration.Value;
            }
            
            _logger.LogDebug("User preference {Key} not found", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preference {Key}", key);
            return null;
        }
    }

    public async Task SetPreferenceAsync(string key, string value)
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var environmentId = await GetUserPreferencesEnvironmentIdAsync();
            var configKey = BuildUserPreferenceKey(currentUser.Username, key);
            
            // Check if preference already exists
            var existingConfig = await _configurationApiClient.GetByKeyAndEnvironmentAsync(configKey, environmentId);

            if (existingConfig != null)
            {
                // Update existing preference
                var updateDto = new UpdateConfigurationDto(
                    value,
                    ConfigurationValueType.String,
                    $"User preference: {key} for {currentUser.Username}"
                );

                await _configurationApiClient.UpdateAsync(existingConfig.Id, updateDto);
                _logger.LogDebug("Updated user preference {Key} = {Value}", key, value);
            }
            else
            {
                // Create new preference
                var createDto = new CreateConfigurationDto(
                    configKey,
                    value,
                    ConfigurationValueType.String,
                    $"User preference: {key} for {currentUser.Username}",
                    Guid.NewGuid(), // TODO: Get actual ApplicationId for user preferences
                    environmentId
                );

                await _configurationApiClient.CreateAsync(createDto);
                _logger.LogDebug("Created user preference {Key} = {Value}", key, value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user preference {Key} = {Value}", key, value);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetAllPreferencesAsync()
    {
        try
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var environmentId = await GetUserPreferencesEnvironmentIdAsync();
            var userKeyPrefix = BuildUserPreferenceKey(currentUser.Username, "");
            
            var allConfigs = await _configurationApiClient.GetByEnvironmentAsync(environmentId);
            
            var userPreferences = allConfigs
                .Where(c => c.Key.StartsWith(userKeyPrefix))
                .ToDictionary(
                    c => ExtractPreferenceKeyFromConfigKey(c.Key, currentUser.Username),
                    c => c.Value
                );
            
            _logger.LogDebug("Retrieved {Count} user preferences", userPreferences.Count);
            return userPreferences;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all user preferences");
            return new Dictionary<string, string>();
        }
    }

    public async Task InitializeUserPreferencesAsync()
    {
        try
        {
            // Ensure the UserPreferences environment exists
            await GetUserPreferencesEnvironmentIdAsync();
            
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            _logger.LogInformation("Initialized user preferences for user: {Username}", currentUser.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing user preferences");
        }
    }

    private async Task<Guid> GetUserPreferencesEnvironmentIdAsync()
    {
        if (_userPreferencesEnvironmentId.HasValue)
        {
            return _userPreferencesEnvironmentId.Value;
        }

        try
        {
            // Try to get existing UserPreferences environment
            var environments = await _environmentApiClient.GetAllAsync();
            var userPrefEnv = environments.FirstOrDefault(e => e.Name == ENVIRONMENT_NAME);

            if (userPrefEnv != null)
            {
                _userPreferencesEnvironmentId = userPrefEnv.Id;
                return _userPreferencesEnvironmentId.Value;
            }

            // Create UserPreferences environment if it doesn't exist
            var createEnvDto = new Application.DTOs.Environment.CreateEnvironmentDto(
                ENVIRONMENT_NAME,
                "Environment for storing user preferences and application settings",
                999 // Put it at the end
            );

            var createdEnv = await _environmentApiClient.CreateAsync(createEnvDto);
            _userPreferencesEnvironmentId = createdEnv.Id;
            
            _logger.LogInformation("Created UserPreferences environment with ID: {EnvironmentId}", _userPreferencesEnvironmentId);
            return _userPreferencesEnvironmentId.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating UserPreferences environment");
            throw;
        }
    }

    private static string BuildUserPreferenceKey(string username, string preferenceKey)
    {
        // Use dots instead of colons to comply with ConfigurationKey validation rules
        // Pattern: UserPreferences.username.preferenceKey (e.g., UserPreferences.john.doe.ThemeMode)
        return $"{USER_PREFERENCES_KEY_PREFIX}.{username}.{preferenceKey}";
    }

    private static string ExtractPreferenceKeyFromConfigKey(string configKey, string username)
    {
        // Updated to use dots instead of colons
        var prefix = $"{USER_PREFERENCES_KEY_PREFIX}.{username}.";
        return configKey.StartsWith(prefix) ? configKey.Substring(prefix.Length) : configKey;
    }
}
