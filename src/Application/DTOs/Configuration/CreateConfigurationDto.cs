using Domain.ValueObjects;

namespace Application.DTOs.Configuration;

public record CreateConfigurationDto(
    string Key,
    string Value,
    ConfigurationValueType ValueType,
    string Description,
    Guid ApplicationId,
    Guid EnvironmentId,
    Guid? GroupId = null,
    bool IsEncrypted = false,
    bool IsRequired = false,
    string? DefaultValue = null);
