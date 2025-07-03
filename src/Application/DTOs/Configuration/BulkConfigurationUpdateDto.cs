namespace Application.DTOs.Configuration;

public record BulkConfigurationUpdateDto(
    Guid[] ConfigurationIds,
    string? NewGroupId = null,
    bool? IsActive = null,
    string? ChangeReason = null);
