using Domain.Entities;

namespace Domain.Repositories;

public interface IApplicationUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByApplicationAndUserAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> GetActiveByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApplicationUser> CreateAsync(ApplicationUser applicationUser, CancellationToken cancellationToken = default);
    Task<ApplicationUser> UpdateAsync(ApplicationUser applicationUser, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasAccessAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid applicationId, Guid userId, string roleName, CancellationToken cancellationToken = default);
}