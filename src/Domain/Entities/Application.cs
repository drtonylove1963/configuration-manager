using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class Application : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ApplicationKey { get; private set; } = string.Empty; // Unique identifier for API access
    public string? ConnectionString { get; private set; } // Application's database connection
    public bool IsActive { get; private set; } = true;
    public DateTime? LastAccessedAt { get; private set; }
    public string? Version { get; private set; }
    public string? Owner { get; private set; } // Business owner
    public string? TechnicalContact { get; private set; }

    // Navigation properties
    private readonly List<Configuration> _configurations = new();
    public IReadOnlyList<Configuration> Configurations => _configurations.AsReadOnly();

    private readonly List<ApplicationUser> _applicationUsers = new();
    public IReadOnlyList<ApplicationUser> ApplicationUsers => _applicationUsers.AsReadOnly();

    private Application() { } // For EF Core

    public Application(string name, string description, string createdBy, string? applicationKey = null)
    {
        var validator = new ApplicationValidator();
        var validationData = new ApplicationValidationData
        {
            Name = name,
            Description = description
        };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid application: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        ApplicationKey = applicationKey ?? GenerateApplicationKey();
        CreatedBy = createdBy;
    }

    public void UpdateDetails(string name, string description, string updatedBy)
    {
        var validator = new ApplicationValidator();
        var validationData = new ApplicationValidationData
        {
            Name = name,
            Description = description
        };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid application: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateConnectionString(string? connectionString, string updatedBy)
    {
        ConnectionString = connectionString;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateVersion(string? version, string updatedBy)
    {
        Version = version;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateContacts(string? owner, string? technicalContact, string updatedBy)
    {
        Owner = owner;
        TechnicalContact = technicalContact;
        MarkAsUpdated(updatedBy);
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        MarkAsUpdated(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        MarkAsUpdated(updatedBy);
    }

    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
    }

    public void RegenerateApplicationKey(string updatedBy)
    {
        ApplicationKey = GenerateApplicationKey();
        MarkAsUpdated(updatedBy);
    }

    public void Delete(string deletedBy)
    {
        MarkAsDeleted(deletedBy);
    }

    private static string GenerateApplicationKey()
    {
        return $"app_{Guid.NewGuid():N}";
    }
}

public class ApplicationValidator : AbstractValidator<ApplicationValidationData>
{
    public ApplicationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Application name is required")
            .MaximumLength(100)
            .WithMessage("Application name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Application description is required")
            .MaximumLength(500)
            .WithMessage("Application description cannot exceed 500 characters");
    }
}

public class ApplicationValidationData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}