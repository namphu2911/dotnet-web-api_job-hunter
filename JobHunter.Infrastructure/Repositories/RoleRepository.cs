using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly JobHunterDbContext _dbContext;

    public RoleRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .Include(r => r.Users)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<Role> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Roles
            .Include(r => r.Permissions)
            .AsNoTracking()
            .AsQueryable();

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "name"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(r => r.Name.ToLower().Contains(term));
        }

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Update(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Roles.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Roles.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    private static IQueryable<Role> ApplySort(IQueryable<Role> query, string? sort)
    {
        if (!SpringFilterQuery.TryParseSort(sort, out var field, out var desc))
        {
            return query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
        }

        return field.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => desc ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
        };
    }
}
