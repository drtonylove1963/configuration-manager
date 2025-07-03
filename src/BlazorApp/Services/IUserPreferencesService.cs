using BlazorApp.Models;

namespace BlazorApp.Services;

public interface IUserPreferencesService
{
    /// <summary>
    /// Get user's theme preference (true = dark mode, false = light mode)
    /// </summary>
    Task<bool> GetThemePreferenceAsync();
    
    /// <summary>
    /// Set user's theme preference
    /// </summary>
    Task SetThemePreferenceAsync(bool isDarkMode);
    
    /// <summary>
    /// Get a user preference value by key
    /// </summary>
    Task<string?> GetPreferenceAsync(string key);
    
    /// <summary>
    /// Set a user preference value by key
    /// </summary>
    Task SetPreferenceAsync(string key, string value);
    
    /// <summary>
    /// Get all user preferences
    /// </summary>
    Task<Dictionary<string, string>> GetAllPreferencesAsync();
    
    /// <summary>
    /// Initialize user preferences (create default preferences if they don't exist)
    /// </summary>
    Task InitializeUserPreferencesAsync();
}
