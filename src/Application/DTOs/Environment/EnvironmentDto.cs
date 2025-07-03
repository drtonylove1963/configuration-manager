namespace Application.DTOs.Environment;

public record EnvironmentDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    int SortOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy,
    int ConfigurationCount = 0);
