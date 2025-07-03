using BlazorApp.Models;

namespace BlazorApp.Services;

public class CurrentUserService : ICurrentUserService
{
    private CurrentUser _currentUser;
    public event Action<CurrentUser>? OnUserChanged;

    public CurrentUserService()
    {
        // Initialize with default demo user for now
        // In a real application, this would be loaded from authentication context
        _currentUser = CurrentUser.CreateDefault();
    }

    public CurrentUser GetCurrentUser()
    {
        return _currentUser;
    }

    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        // Simulate async operation - in real app this might call an API
        await Task.Delay(10);
        return _currentUser;
    }

    public void SetCurrentUser(CurrentUser user)
    {
        _currentUser = user;
        OnUserChanged?.Invoke(_currentUser);
    }
}
