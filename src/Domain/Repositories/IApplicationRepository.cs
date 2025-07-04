using Domain.Entities;

namespace Domain.Repositories;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Application?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Application?> GetByApplicationKeyAsync(string applicationKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Application>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Application>> GetActiveApplicationsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Application>> GetApplicationsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Application> CreateAsync(Application application, CancellationToken cancellationToken = default);
    Task<Application> UpdateAsync(Application application, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ApplicationKeyExistsAsync(string applicationKey, CancellationToken cancellationToken = default);
}