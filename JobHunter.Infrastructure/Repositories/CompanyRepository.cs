using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly JobHunterDbContext _dbContext;

    public CompanyRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .Include(c => c.Users)
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<Company> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Companies
            .AsNoTracking()
            .AsQueryable();

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "name"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "address"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(c => c.Address != null && c.Address.ToLower().Contains(term));
        }

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        _dbContext.Companies.Update(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Companies.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Companies.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static IQueryable<Company> ApplySort(IQueryable<Company> query, string? sort)
    {
        if (!SpringFilterQuery.TryParseSort(sort, out var field, out var desc))
        {
            return query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
        }

        return field.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "address" => desc ? query.OrderByDescending(x => x.Address) : query.OrderBy(x => x.Address),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => desc ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
        };
    }
}
