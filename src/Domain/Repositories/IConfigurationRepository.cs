using Domain.Entities;

namespace Domain.Repositories;

public interface IConfigurationRepository
{
    Task<Configuration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Configuration?> GetByKeyAndEnvironmentAsync(string key, Guid environmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Configuration>> GetByEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Configuration>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Configuration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Configuration>> SearchAsync(string searchTerm, Guid? environmentId = null, Guid? groupId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, Guid environmentId, CancellationToken cancellationToken = default);
    Task AddAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task DeleteAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationHistory>> GetHistoryAsync(Guid configurationId, CancellationToken cancellationToken = default);
}

public interface IEnvironmentRepository
{
    Task<Domain.Entities.Environment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Environment?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Environment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Environment>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Environment environment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.Environment environment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.Environment environment, CancellationToken cancellationToken = default);
}

public interface IConfigurationGroupRepository
{
    Task<ConfigurationGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConfigurationGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroup>> GetByParentAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationGroup>> GetRootGroupsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(ConfigurationGroup group, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConfigurationGroup group, CancellationToken cancellationToken = default);
    Task DeleteAsync(ConfigurationGroup group, CancellationToken cancellationToken = default);
}
