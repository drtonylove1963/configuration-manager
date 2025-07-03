namespace Application.DTOs.ConfigurationGroup;

public record CreateConfigurationGroupDto(
    string Name,
    string Description,
    Guid? ParentGroupId = null,
    int SortOrder = 0);
