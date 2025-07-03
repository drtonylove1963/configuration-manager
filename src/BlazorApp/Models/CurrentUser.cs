namespace BlazorApp.Models;

public class CurrentUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username;
    public string Initials => GetInitials();

    private string GetInitials()
    {
        if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
        {
            return $"{FirstName[0]}{LastName[0]}".ToUpper();
        }
        if (!string.IsNullOrEmpty(Username) && Username.Length >= 2)
        {
            return Username.Substring(0, 2).ToUpper();
        }
        return "U";
    }

    public static CurrentUser CreateDefault()
    {
        return new CurrentUser
        {
            Id = "demo-user-001",
            Username = "john.doe",
            Email = "john.doe@company.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Administrator",
            AvatarUrl = "",
            IsAuthenticated = true,
            LastLoginAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static CurrentUser CreateGuest()
    {
        return new CurrentUser
        {
            Id = "guest",
            Username = "guest",
            Email = "",
            FirstName = "Guest",
            LastName = "User",
            Role = "Viewer",
            AvatarUrl = "",
            IsAuthenticated = false,
            LastLoginAt = null
        };
    }
}
