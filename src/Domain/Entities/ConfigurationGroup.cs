using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class ConfigurationGroup : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? ParentGroupId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private readonly List<Configuration> _configurations = new();
    public IReadOnlyList<Configuration> Configurations => _configurations.AsReadOnly();

    private readonly List<ConfigurationGroup> _childGroups = new();
    public IReadOnlyList<ConfigurationGroup> ChildGroups => _childGroups.AsReadOnly();

    private ConfigurationGroup() { } // For EF Core

    public ConfigurationGroup(string name, string description, string createdBy, Guid? parentGroupId = null, int sortOrder = 0)
    {
        var validator = new ConfigurationGroupValidator();
        var validationData = new ConfigurationGroupValidationData { Name = name, Description = description };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid configuration group: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        ParentGroupId = parentGroupId;
        CreatedBy = createdBy;
        SortOrder = sortOrder;
    }

    public void UpdateDetails(string name, string description, string updatedBy, int sortOrder = 0)
    {
        var validator = new ConfigurationGroupValidator();
        var validationData = new ConfigurationGroupValidationData { Name = name, Description = description };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid configuration group: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        SortOrder = sortOrder;
        MarkAsUpdated(updatedBy);
    }

    public void ChangeParent(Guid? parentGroupId, string updatedBy)
    {
        ParentGroupId = parentGroupId;
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

public class ConfigurationGroupValidationData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ConfigurationGroupValidator : AbstractValidator<ConfigurationGroupValidationData>
{
    public ConfigurationGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Configuration group name is required")
            .MaximumLength(100)
            .WithMessage("Configuration group name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Configuration group description cannot exceed 500 characters");
    }
}
