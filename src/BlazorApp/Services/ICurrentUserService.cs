using BlazorApp.Models;

namespace BlazorApp.Services;

public interface ICurrentUserService
{
    CurrentUser GetCurrentUser();
    Task<CurrentUser> GetCurrentUserAsync();
    void SetCurrentUser(CurrentUser user);
    event Action<CurrentUser>? OnUserChanged;
}
