using Application.DTOs.ConfigurationGroup;

namespace Application.Interfaces;

public interface IConfigurationGroupService
{
    Task<ConfigurationGroupDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConfigurationGroupDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroupDto>> GetByParentAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroupDto>> GetRootGroupsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroupTreeDto>> GetGroupTreeAsync(CancellationToken cancellationToken = default);
    
    Task<ConfigurationGroupDto> CreateAsync(CreateConfigurationGroupDto createDto, string createdBy, CancellationToken cancellationToken = default);
    Task<ConfigurationGroupDto> UpdateAsync(Guid id, UpdateConfigurationGroupDto updateDto, string updatedBy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    
    Task<ConfigurationGroupDto> MoveGroupAsync(Guid id, MoveGroupDto moveDto, string updatedBy, CancellationToken cancellationToken = default);
    
    Task ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
