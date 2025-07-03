namespace Application.DTOs.ConfigurationGroup;

public record UpdateConfigurationGroupDto(
    string Name,
    string Description,
    int SortOrder = 0);
