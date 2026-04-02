using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class SubscriberRepository : ISubscriberRepository
{
    private readonly JobHunterDbContext _dbContext;

    public SubscriberRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Subscriber?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscribers
            .Include(s => s.Skills)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<Subscriber> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Subscribers
            .Include(s => s.Skills)
            .AsNoTracking()
            .AsQueryable();

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "name"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(s => s.Name.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "email"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(s => s.Email.ToLower().Contains(term));
        }

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<List<Subscriber>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscribers
            .Include(s => s.Skills)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subscriber subscriber, CancellationToken cancellationToken = default)
    {
        _dbContext.Subscribers.Add(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Subscriber subscriber, CancellationToken cancellationToken = default)
    {
        _dbContext.Subscribers.Update(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Subscribers.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Subscribers.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscribers.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<Subscriber?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscribers
            .Include(s => s.Skills)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    private static IQueryable<Subscriber> ApplySort(IQueryable<Subscriber> query, string? sort)
    {
        if (!SpringFilterQuery.TryParseSort(sort, out var field, out var desc))
        {
            return query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
        }

        return field.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => desc ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
        };
    }
}
