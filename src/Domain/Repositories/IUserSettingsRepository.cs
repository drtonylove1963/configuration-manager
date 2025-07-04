using Domain.Entities;

namespace Domain.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSettings> CreateAsync(UserSettings userSettings, CancellationToken cancellationToken = default);
    Task<UserSettings> UpdateAsync(UserSettings userSettings, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}