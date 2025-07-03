using Domain.ValueObjects;

namespace Application.DTOs.Configuration;

public record ConfigurationSearchDto(
    string? SearchTerm = null,
    Guid? EnvironmentId = null,
    Guid? GroupId = null,
    bool? IsActive = null,
    ConfigurationValueType? ValueType = null);
