namespace Application.DTOs.Environment;

public record UpdateEnvironmentDto(
    string Name,
    string Description,
    int SortOrder = 0);
