namespace Application.DTOs.Auth;

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<ApplicationAccess> Applications { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsEmailConfirmed { get; set; }
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
}

public class ApplicationAccess
{
    public string ApplicationId { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationKey { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}
