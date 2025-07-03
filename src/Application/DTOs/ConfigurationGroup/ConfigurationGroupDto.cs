namespace Application.DTOs.ConfigurationGroup;

public record ConfigurationGroupDto(
    Guid Id,
    string Name,
    string Description,
    Guid? ParentGroupId,
    string? ParentGroupName,
    bool IsActive,
    int SortOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy,
    int ConfigurationCount = 0,
    int ChildGroupCount = 0);
