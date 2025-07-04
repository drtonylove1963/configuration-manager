using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly ConfigurationDbContext _context;

    public ApplicationUserRepository(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Application)
            .Include(au => au.User)
            .Include(au => au.Role)
            .FirstOrDefaultAsync(au => au.Id == id && !au.IsDeleted, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByApplicationAndUserAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Application)
            .Include(au => au.User)
            .Include(au => au.Role)
            .FirstOrDefaultAsync(au => au.ApplicationId == applicationId && au.UserId == userId && !au.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<ApplicationUser>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Application)
            .Include(au => au.User)
            .Include(au => au.Role)
            .Where(au => au.ApplicationId == applicationId && !au.IsDeleted)
            .OrderBy(au => au.User.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationUser>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Application)
            .Include(au => au.User)
            .Include(au => au.Role)
            .Where(au => au.UserId == userId && !au.IsDeleted)
            .OrderBy(au => au.Application.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationUser>> GetActiveByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Application)
            .Include(au => au.User)
            .Include(au => au.Role)
            .Where(au => au.ApplicationId == applicationId && au.IsActive && !au.IsDeleted)
            .OrderBy(au => au.User.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationUser>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Application)
            .Include(au => au.User)
            .Include(au => au.Role)
            .Where(au => au.UserId == userId && au.IsActive && !au.IsDeleted)
            .OrderBy(au => au.Application.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationUser> CreateAsync(ApplicationUser applicationUser, CancellationToken cancellationToken = default)
    {
        _context.ApplicationUsers.Add(applicationUser);
        await _context.SaveChangesAsync(cancellationToken);
        return applicationUser;
    }

    public async Task<ApplicationUser> UpdateAsync(ApplicationUser applicationUser, CancellationToken cancellationToken = default)
    {
        _context.ApplicationUsers.Update(applicationUser);
        await _context.SaveChangesAsync(cancellationToken);
        return applicationUser;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var applicationUser = await GetByIdAsync(id, cancellationToken);
        if (applicationUser != null)
        {
            applicationUser.Delete("system"); // TODO: Get actual user
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .AnyAsync(au => au.ApplicationId == applicationId && au.UserId == userId && !au.IsDeleted, cancellationToken);
    }

    public async Task<bool> HasAccessAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .AnyAsync(au => au.ApplicationId == applicationId && au.UserId == userId && au.IsActive && !au.IsDeleted, cancellationToken);
    }

    public async Task<bool> HasRoleAsync(Guid applicationId, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        return await _context.ApplicationUsers
            .Include(au => au.Role)
            .AnyAsync(au => au.ApplicationId == applicationId && 
                           au.UserId == userId && 
                           au.Role.Name == roleName && 
                           au.IsActive && 
                           !au.IsDeleted, cancellationToken);
    }
}
