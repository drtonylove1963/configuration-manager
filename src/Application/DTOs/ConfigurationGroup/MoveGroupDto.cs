namespace Application.DTOs.ConfigurationGroup;

public record MoveGroupDto(
    Guid? NewParentGroupId = null);
