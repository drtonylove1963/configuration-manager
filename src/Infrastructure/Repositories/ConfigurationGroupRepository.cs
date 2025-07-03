using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class ConfigurationGroupRepository : IConfigurationGroupRepository
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<ConfigurationGroupRepository> _logger;

    public ConfigurationGroupRepository(ConfigurationDbContext context, ILogger<ConfigurationGroupRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ConfigurationGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationGroups
            .Include(g => g.Configurations)
            .Include(g => g.ChildGroups)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<ConfigurationGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationGroups
            .Include(g => g.Configurations)
            .Include(g => g.ChildGroups)
            .FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<ConfigurationGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationGroups
            .Include(g => g.Configurations)
            .Include(g => g.ChildGroups)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ConfigurationGroup>> GetByParentAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationGroups
            .Include(g => g.Configurations)
            .Include(g => g.ChildGroups)
            .Where(g => g.ParentGroupId == parentId)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ConfigurationGroup>> GetRootGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationGroups
            .Include(g => g.Configurations)
            .Include(g => g.ChildGroups)
            .Where(g => g.ParentGroupId == null)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationGroups
            .AnyAsync(g => g.Name == name, cancellationToken);
    }

    public async Task AddAsync(ConfigurationGroup group, CancellationToken cancellationToken = default)
    {
        _context.ConfigurationGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Configuration group {Name} added", group.Name);
    }

    public async Task UpdateAsync(ConfigurationGroup group, CancellationToken cancellationToken = default)
    {
        _context.ConfigurationGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Configuration group {Name} updated", group.Name);
    }

    public async Task DeleteAsync(ConfigurationGroup group, CancellationToken cancellationToken = default)
    {
        _context.ConfigurationGroups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Configuration group {Name} deleted", group.Name);
    }
}
