using Domain.ValueObjects;

namespace Application.DTOs.Configuration;

public record ConfigurationDto(
    Guid Id,
    string Key,
    string Value,
    ConfigurationValueType ValueType,
    string Description,
    Guid ApplicationId,
    string ApplicationName,
    Guid EnvironmentId,
    string EnvironmentName,
    Guid? GroupId,
    string? GroupName,
    bool IsEncrypted,
    bool IsRequired,
    string? DefaultValue,
    bool IsActive,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy);
