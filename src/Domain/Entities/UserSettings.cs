using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class UserSettings : BaseEntity
{
    public Guid UserId { get; private set; }
    public string ThemeMode { get; private set; } = "light"; // "light" or "dark"
    public Guid? LastSelectedEnvironmentId { get; private set; }
    public string? LastSelectedDatabase { get; private set; }
    public int PageSize { get; private set; } = 25;
    public string? DefaultLanguage { get; private set; } = "en";
    public string? TimeZone { get; private set; }
    public bool EnableNotifications { get; private set; } = true;
    public bool EnableEmailNotifications { get; private set; } = false;
    public string? CustomSettings { get; private set; } // JSON for additional settings

    // Navigation properties
    public User User { get; private set; } = null!;
    public Domain.Entities.Environment? LastSelectedEnvironment { get; private set; }

    private UserSettings() { } // For EF Core

    public UserSettings(Guid userId, string createdBy)
    {
        UserId = userId;
        CreatedBy = createdBy;
    }

    public void UpdateTheme(string themeMode, string updatedBy)
    {
        if (themeMode != "light" && themeMode != "dark")
            throw new ArgumentException("Theme mode must be 'light' or 'dark'", nameof(themeMode));

        ThemeMode = themeMode;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateLastSelectedEnvironment(Guid? environmentId, string updatedBy)
    {
        LastSelectedEnvironmentId = environmentId;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateLastSelectedDatabase(string? database, string updatedBy)
    {
        LastSelectedDatabase = database;
        MarkAsUpdated(updatedBy);
    }

    public void UpdatePageSize(int pageSize, string updatedBy)
    {
        if (pageSize < 5 || pageSize > 100)
            throw new ArgumentException("Page size must be between 5 and 100", nameof(pageSize));

        PageSize = pageSize;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateLanguage(string? language, string updatedBy)
    {
        DefaultLanguage = language;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateTimeZone(string? timeZone, string updatedBy)
    {
        TimeZone = timeZone;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateNotificationSettings(bool enableNotifications, bool enableEmailNotifications, string updatedBy)
    {
        EnableNotifications = enableNotifications;
        EnableEmailNotifications = enableEmailNotifications;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateCustomSettings(string? customSettings, string updatedBy)
    {
        CustomSettings = customSettings;
        MarkAsUpdated(updatedBy);
    }
}

public class UserSettingsValidator : AbstractValidator<UserSettingsValidationData>
{
    public UserSettingsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.ThemeMode)
            .Must(x => x == "light" || x == "dark")
            .WithMessage("Theme mode must be 'light' or 'dark'");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(5, 100)
            .WithMessage("Page size must be between 5 and 100");
    }
}

public class UserSettingsValidationData
{
    public Guid UserId { get; set; }
    public string ThemeMode { get; set; } = "light";
    public int PageSize { get; set; } = 25;
}