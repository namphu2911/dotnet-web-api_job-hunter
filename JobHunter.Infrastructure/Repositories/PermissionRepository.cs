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
            .Include(p => p.Roles)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<Permission> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Permissions
            .AsNoTracking()
            .AsQueryable();

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "name"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "apipath"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(p => p.ApiPath.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "method"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(p => p.Method.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "module"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(p => p.Module.ToLower().Contains(term));
        }

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
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

    private static IQueryable<Permission> ApplySort(IQueryable<Permission> query, string? sort)
    {
        if (!SpringFilterQuery.TryParseSort(sort, out var field, out var desc))
        {
            return query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
        }

        return field.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "apipath" => desc ? query.OrderByDescending(x => x.ApiPath) : query.OrderBy(x => x.ApiPath),
            "method" => desc ? query.OrderByDescending(x => x.Method) : query.OrderBy(x => x.Method),
            "module" => desc ? query.OrderByDescending(x => x.Module) : query.OrderBy(x => x.Module),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => desc ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
        };
    }
}
