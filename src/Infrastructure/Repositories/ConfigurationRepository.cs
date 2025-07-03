using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<ConfigurationRepository> _logger;

    public ConfigurationRepository(ConfigurationDbContext context, ILogger<ConfigurationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Configuration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Configurations
            .Include(c => c.Environment)
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Configuration?> GetByKeyAndEnvironmentAsync(string key, Guid environmentId, CancellationToken cancellationToken = default)
    {
        var configurations = await _context.Configurations
            .Include(c => c.Environment)
            .Include(c => c.Group)
            .Where(c => c.EnvironmentId == environmentId)
            .ToListAsync(cancellationToken);

        return configurations.FirstOrDefault(c => c.Key.Value == key);
    }

    public async Task<IEnumerable<Configuration>> GetByEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken = default)
    {
        var configurations = await _context.Configurations
            .Include(c => c.Environment)
            .Include(c => c.Group)
            .Where(c => c.EnvironmentId == environmentId)
            .ToListAsync(cancellationToken);

        return configurations.OrderBy(c => c.Key.Value);
    }

    public async Task<IEnumerable<Configuration>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var configurations = await _context.Configurations
            .Include(c => c.Environment)
            .Include(c => c.Group)
            .Where(c => c.GroupId == groupId)
            .ToListAsync(cancellationToken);

        return configurations.OrderBy(c => c.Key.Value);
    }

    public async Task<IEnumerable<Configuration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var configurations = await _context.Configurations
            .Include(c => c.Environment)
            .Include(c => c.Group)
            .ToListAsync(cancellationToken);

        return configurations
            .OrderBy(c => c.Environment.Name)
            .ThenBy(c => c.Key.Value);
    }

    public async Task<IEnumerable<Configuration>> SearchAsync(string searchTerm, Guid? environmentId = null, Guid? groupId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Configurations
            .Include(c => c.Environment)
            .Include(c => c.Group)
            .AsQueryable();

        if (environmentId.HasValue)
        {
            query = query.Where(c => c.EnvironmentId == environmentId.Value);
        }

        if (groupId.HasValue)
        {
            query = query.Where(c => c.GroupId == groupId.Value);
        }

        var configurations = await query.ToListAsync(cancellationToken);

        // Apply search filter and sorting in memory to avoid value object translation issues
        if (!string.IsNullOrEmpty(searchTerm))
        {
            configurations = configurations.Where(c =>
                c.Key.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Value.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return configurations
            .OrderBy(c => c.Environment.Name)
            .ThenBy(c => c.Key.Value);
    }

    public async Task<bool> ExistsAsync(string key, Guid environmentId, CancellationToken cancellationToken = default)
    {
        var configurations = await _context.Configurations
            .Where(c => c.EnvironmentId == environmentId)
            .ToListAsync(cancellationToken);

        return configurations.Any(c => c.Key.Value == key);
    }

    public async Task AddAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Configuration {Key} added to environment {EnvironmentId}", 
            configuration.Key.Value, configuration.EnvironmentId);
    }

    public async Task UpdateAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Configuration {Key} updated in environment {EnvironmentId}", 
            configuration.Key.Value, configuration.EnvironmentId);
    }

    public async Task DeleteAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        _context.Configurations.Remove(configuration);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Configuration {Key} deleted from environment {EnvironmentId}", 
            configuration.Key.Value, configuration.EnvironmentId);
    }

    public async Task<IEnumerable<ConfigurationHistory>> GetHistoryAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationHistory
            .Where(h => h.ConfigurationId == configurationId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
