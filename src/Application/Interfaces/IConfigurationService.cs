using Application.DTOs.Configuration;

namespace Application.Interfaces;

public interface IConfigurationService
{
    Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConfigurationDto?> GetByKeyAndEnvironmentAsync(string key, Guid environmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationDto>> GetByEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationDto>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationDto>> SearchAsync(ConfigurationSearchDto searchDto, CancellationToken cancellationToken = default);
    
    Task<ConfigurationDto> CreateAsync(CreateConfigurationDto createDto, string createdBy, CancellationToken cancellationToken = default);
    Task<ConfigurationDto> UpdateAsync(Guid id, UpdateConfigurationDto updateDto, string updatedBy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ConfigurationHistoryDto>> GetHistoryAsync(Guid configurationId, CancellationToken cancellationToken = default);
    
    Task ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
    
    Task BulkUpdateAsync(BulkConfigurationUpdateDto bulkUpdateDto, string updatedBy, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string key, Guid environmentId, CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, string>> GetEnvironmentConfigurationsAsync(Guid environmentId, bool activeOnly = true, CancellationToken cancellationToken = default);
}
