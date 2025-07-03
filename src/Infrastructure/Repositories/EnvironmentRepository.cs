using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class EnvironmentRepository : IEnvironmentRepository
{
    private readonly ConfigurationDbContext _context;
    private readonly ILogger<EnvironmentRepository> _logger;

    public EnvironmentRepository(ConfigurationDbContext context, ILogger<EnvironmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Domain.Entities.Environment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Environments
            .Include(e => e.Configurations)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.Environment?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Environments
            .Include(e => e.Configurations)
            .FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Environment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Environments
            .Include(e => e.Configurations)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Environment>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Environments
            .Include(e => e.Configurations)
            .Where(e => e.IsActive)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Environments
            .AnyAsync(e => e.Name == name, cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.Environment environment, CancellationToken cancellationToken = default)
    {
        _context.Environments.Add(environment);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Environment {Name} added", environment.Name);
    }

    public async Task UpdateAsync(Domain.Entities.Environment environment, CancellationToken cancellationToken = default)
    {
        _context.Environments.Update(environment);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Environment {Name} updated", environment.Name);
    }

    public async Task DeleteAsync(Domain.Entities.Environment environment, CancellationToken cancellationToken = default)
    {
        _context.Environments.Remove(environment);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Environment {Name} deleted", environment.Name);
    }
}
