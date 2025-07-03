namespace Application.DTOs.Environment;

public record CreateEnvironmentDto(
    string Name,
    string Description,
    int SortOrder = 0);
