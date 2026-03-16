using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly JobHunterDbContext _dbContext;

    public PermissionRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Permissions.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Permissions.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByModuleApiPathMethodAsync(string module, string apiPath, string method, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions.AnyAsync(x => x.Module == module && x.ApiPath == apiPath && x.Method == method, cancellationToken);
    }

    public async Task<List<Permission>> FindByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }
}
