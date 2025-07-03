namespace Application.DTOs.Environment;

public record EnvironmentSummaryDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    int ConfigurationCount,
    DateTime LastUpdated);
