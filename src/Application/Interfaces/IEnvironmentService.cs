using Application.DTOs.Environment;

namespace Application.Interfaces;

public interface IEnvironmentService
{
    Task<EnvironmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EnvironmentDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnvironmentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EnvironmentDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EnvironmentSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken = default);
    
    Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto createDto, string createdBy, CancellationToken cancellationToken = default);
    Task<EnvironmentDto> UpdateAsync(Guid id, UpdateEnvironmentDto updateDto, string updatedBy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    
    Task ActivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}
