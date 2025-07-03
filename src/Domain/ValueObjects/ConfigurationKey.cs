using FluentValidation;

namespace Domain.ValueObjects;

public record ConfigurationKey
{
    public string Value { get; }

    private ConfigurationKey(string value)
    {
        Value = value;
    }

    public static ConfigurationKey Create(string value)
    {
        if (value == null)
        {
            throw new ArgumentException("Configuration key cannot be null");
        }

        var validator = new ConfigurationKeyValidator();
        var result = validator.Validate(value);

        if (!result.IsValid)
        {
            throw new ArgumentException($"Invalid configuration key: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
        }

        return new ConfigurationKey(value);
    }

    public static implicit operator string(ConfigurationKey key) => key.Value;
    public override string ToString() => Value;
}

public class ConfigurationKeyValidator : AbstractValidator<string>
{
    public ConfigurationKeyValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Configuration key cannot be empty")
            .MaximumLength(200)
            .WithMessage("Configuration key cannot exceed 200 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9._-]*$")
            .WithMessage("Configuration key must start with a letter and contain only letters, numbers, dots, underscores, and hyphens");
    }
}
