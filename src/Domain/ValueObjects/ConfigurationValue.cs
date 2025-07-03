using FluentValidation;

namespace Domain.ValueObjects;

public record ConfigurationValue
{
    public string Value { get; }
    public ConfigurationValueType Type { get; }

    private ConfigurationValue(string value, ConfigurationValueType type)
    {
        Value = value;
        Type = type;
    }

    public static ConfigurationValue Create(string value, ConfigurationValueType type)
    {
        var validator = new ConfigurationValueValidator();
        var validationContext = new ValidationContext<(string, ConfigurationValueType)>((value, type));
        var result = validator.Validate(validationContext);
        
        if (!result.IsValid)
        {
            throw new ArgumentException($"Invalid configuration value: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
        }

        return new ConfigurationValue(value, type);
    }

    public T GetTypedValue<T>()
    {
        return Type switch
        {
            ConfigurationValueType.String => (T)(object)Value,
            ConfigurationValueType.Integer => (T)(object)int.Parse(Value),
            ConfigurationValueType.Boolean => (T)(object)bool.Parse(Value),
            ConfigurationValueType.Decimal => (T)(object)decimal.Parse(Value),
            ConfigurationValueType.DateTime => (T)(object)DateTime.Parse(Value),
            ConfigurationValueType.Json => (T)(object)Value,
            _ => throw new InvalidOperationException($"Unsupported type: {Type}")
        };
    }

    public static implicit operator string(ConfigurationValue configValue) => configValue.Value;
    public override string ToString() => Value;
}

public enum ConfigurationValueType
{
    String,
    Integer,
    Boolean,
    Decimal,
    DateTime,
    Json
}

public class ConfigurationValueValidator : AbstractValidator<(string Value, ConfigurationValueType Type)>
{
    public ConfigurationValueValidator()
    {
        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage("Configuration value cannot be null");

        RuleFor(x => x)
            .Must(x => ValidateValueForType(x.Value, x.Type))
            .WithMessage(x => $"Value '{x.Value}' is not valid for type {x.Type}");
    }

    private static bool ValidateValueForType(string value, ConfigurationValueType type)
    {
        if (string.IsNullOrEmpty(value) && type != ConfigurationValueType.String)
            return false;

        return type switch
        {
            ConfigurationValueType.String => true,
            ConfigurationValueType.Integer => int.TryParse(value, out _),
            ConfigurationValueType.Boolean => bool.TryParse(value, out _),
            ConfigurationValueType.Decimal => decimal.TryParse(value, out _),
            ConfigurationValueType.DateTime => DateTime.TryParse(value, out _),
            ConfigurationValueType.Json => IsValidJson(value),
            _ => false
        };
    }

    private static bool IsValidJson(string value)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
