using Domain.Common;
using Domain.ValueObjects;
using FluentValidation;

namespace Domain.Entities;

public class Configuration : BaseEntity
{
    public ConfigurationKey Key { get; private set; } = null!;
    public ConfigurationValue Value { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public Guid ApplicationId { get; private set; } // Multi-tenant: Configuration belongs to Application
    public Guid EnvironmentId { get; private set; }
    public Guid? GroupId { get; private set; }
    public bool IsEncrypted { get; private set; }
    public bool IsRequired { get; private set; }
    public string? DefaultValue { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Version { get; private set; } = 1;

    // Navigation properties
    public Application Application { get; private set; } = null!;
    public Environment Environment { get; private set; } = null!;
    public ConfigurationGroup? Group { get; private set; }

    private readonly List<ConfigurationHistory> _history = new();
    public IReadOnlyList<ConfigurationHistory> History => _history.AsReadOnly();

    private Configuration() { } // For EF Core

    public Configuration(
        string key,
        string value,
        ConfigurationValueType valueType,
        string description,
        Guid applicationId,
        Guid environmentId,
        string createdBy,
        Guid? groupId = null,
        bool isEncrypted = false,
        bool isRequired = false,
        string? defaultValue = null)
    {
        var validator = new ConfigurationValidator();
        var validationData = new ConfigurationValidationData
        {
            Key = key,
            Value = value,
            Description = description,
            ApplicationId = applicationId,
            EnvironmentId = environmentId
        };

        var validationResult = validator.Validate(validationData);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid configuration: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Key = ConfigurationKey.Create(key);
        Value = ConfigurationValue.Create(value, valueType);
        Description = description;
        ApplicationId = applicationId;
        EnvironmentId = environmentId;
        GroupId = groupId;
        IsEncrypted = isEncrypted;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        CreatedBy = createdBy;
    }

    public void UpdateValue(string value, ConfigurationValueType valueType, string updatedBy, string? changeReason = null)
    {
        var oldValue = Value;
        Value = ConfigurationValue.Create(value, valueType);
        Version++;
        
        // Add to history
        _history.Add(new ConfigurationHistory(
            Id,
            oldValue.Value,
            oldValue.Type,
            value,
            valueType,
            updatedBy,
            changeReason));
        
        MarkAsUpdated(updatedBy);
    }

    public void UpdateDetails(string description, string updatedBy, Guid? groupId = null, bool? isRequired = null, string? defaultValue = null)
    {
        Description = description;
        if (groupId.HasValue) GroupId = groupId;
        if (isRequired.HasValue) IsRequired = isRequired.Value;
        if (defaultValue != null) DefaultValue = defaultValue;
        
        MarkAsUpdated(updatedBy);
    }

    public void ToggleEncryption(string updatedBy)
    {
        IsEncrypted = !IsEncrypted;
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

public class ConfigurationValidationData
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public Guid EnvironmentId { get; set; }
}

public class ConfigurationValidator : AbstractValidator<ConfigurationValidationData>
{
    public ConfigurationValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Configuration key is required");

        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage("Configuration value is required");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Configuration description cannot exceed 1000 characters");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required");

        RuleFor(x => x.EnvironmentId)
            .NotEmpty()
            .WithMessage("Environment ID is required");
    }
}
