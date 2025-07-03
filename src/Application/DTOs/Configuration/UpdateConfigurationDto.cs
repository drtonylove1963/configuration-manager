using Domain.ValueObjects;

namespace Application.DTOs.Configuration;

public record UpdateConfigurationDto(
    string Value,
    ConfigurationValueType ValueType,
    string Description,
    Guid? GroupId = null,
    bool? IsRequired = null,
    string? DefaultValue = null,
    string? ChangeReason = null);
