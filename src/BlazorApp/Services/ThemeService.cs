using Microsoft.JSInterop;

namespace BlazorApp.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    event Action? ThemeChanged;
    Task ToggleThemeAsync();
    Task SetThemeAsync(string theme);
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ThemeService> _logger;
    private event Action? _themeChanged;
    private string _currentTheme = "light";

    public ThemeService(IJSRuntime jsRuntime, ILogger<ThemeService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public string CurrentTheme => _currentTheme;

    public event Action? ThemeChanged
    {
        add => _themeChanged += value;
        remove => _themeChanged -= value;
    }

    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme == "light" ? "dark" : "light";
        await SetThemeAsync(newTheme);
    }

    public async Task SetThemeAsync(string theme)
    {
        // Simple validation - only allow 'light' or 'dark'
        if (theme != "light" && theme != "dark")
        {
            _logger.LogWarning("Invalid theme '{Theme}' requested, defaulting to light", theme);
            theme = "light";
        }

        _currentTheme = theme;
        _logger.LogInformation("Setting theme to: {Theme}", theme);

        try
        {
            // Call the simple JavaScript function
            await _jsRuntime.InvokeVoidAsync("setTheme", theme);
            _logger.LogInformation("Theme applied successfully: {Theme}", theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying theme: {Theme}", theme);
        }

        // Notify components of theme change
        _themeChanged?.Invoke();
    }
}
