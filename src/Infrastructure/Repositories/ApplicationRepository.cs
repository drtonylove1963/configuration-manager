using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ConfigurationDbContext _context;

    public ApplicationRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.User)
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.Role)
            .Include(a => a.Configurations)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<Domain.Entities.Application?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.User)
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.Role)
            .FirstOrDefaultAsync(a => a.Name == name && !a.IsDeleted, cancellationToken);
    }

    public async Task<Domain.Entities.Application?> GetByApplicationKeyAsync(string applicationKey, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.User)
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.Role)
            .FirstOrDefaultAsync(a => a.ApplicationKey == applicationKey && !a.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Application>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.User)
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.Role)
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Application>> GetActiveApplicationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.User)
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.Role)
            .Where(a => a.IsActive && !a.IsDeleted)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Application>> GetApplicationsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.User)
            .Include(a => a.ApplicationUsers)
                .ThenInclude(au => au.Role)
            .Where(a => a.ApplicationUsers.Any(au => au.UserId == userId && au.IsActive && !au.IsDeleted) && !a.IsDeleted)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Application> CreateAsync(Domain.Entities.Application application, CancellationToken cancellationToken = default)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);
        return application;
    }

    public async Task<Domain.Entities.Application> UpdateAsync(Domain.Entities.Application application, CancellationToken cancellationToken = default)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync(cancellationToken);
        return application;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await GetByIdAsync(id, cancellationToken);
        if (application != null)
        {
            application.Delete("system"); // TODO: Get actual user
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.Name == name && !a.IsDeleted, cancellationToken);
    }

    public async Task<bool> ApplicationKeyExistsAsync(string applicationKey, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.ApplicationKey == applicationKey && !a.IsDeleted, cancellationToken);
    }
}
