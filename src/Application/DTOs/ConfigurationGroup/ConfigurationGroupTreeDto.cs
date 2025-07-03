namespace Application.DTOs.ConfigurationGroup;

public record ConfigurationGroupTreeDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    int ConfigurationCount,
    List<ConfigurationGroupTreeDto> Children = default!)
{
    public List<ConfigurationGroupTreeDto> Children { get; init; } = Children ?? new List<ConfigurationGroupTreeDto>();
    public bool IsExpanded { get; set; } = false;
    public int ChildGroupCount => Children?.Count ?? 0;
}
