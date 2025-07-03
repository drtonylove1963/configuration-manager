using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class Environment : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private readonly List<Configuration> _configurations = new();
    public IReadOnlyList<Configuration> Configurations => _configurations.AsReadOnly();

    private Environment() { } // For EF Core

    public Environment(string name, string description, string createdBy, int sortOrder = 0)
    {
        var validator = new EnvironmentValidator();
        var validationData = new EnvironmentValidationData { Name = name, Description = description };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid environment: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        CreatedBy = createdBy;
        SortOrder = sortOrder;
    }

    public void UpdateDetails(string name, string description, string updatedBy, int sortOrder = 0)
    {
        var validator = new EnvironmentValidator();
        var validationData = new EnvironmentValidationData { Name = name, Description = description };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid environment: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        SortOrder = sortOrder;
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

    public void Delete(string deletedBy)
    {
        MarkAsDeleted(deletedBy);
    }
}

public class EnvironmentValidationData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EnvironmentValidator : AbstractValidator<EnvironmentValidationData>
{
    public EnvironmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Environment name is required")
            .MaximumLength(100)
            .WithMessage("Environment name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_-]*$")
            .WithMessage("Environment name must start with a letter and contain only letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Environment description cannot exceed 500 characters");
    }
}
