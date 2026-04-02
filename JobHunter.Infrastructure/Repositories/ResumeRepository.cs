using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class ResumeRepository : IResumeRepository
{
    private readonly JobHunterDbContext _dbContext;

    public ResumeRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<Resume> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Resumes
            .Include(r => r.User)
            .Include(r => r.Job).ThenInclude(j => j.Company)
            .AsNoTracking()
            .AsQueryable();

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "email"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(r => r.Email.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "status"))
        {
            var term = value.ToUpperInvariant();
            query = query.Where(r => r.Status.ToString().ToUpper().Contains(term));
        }

        var statusInValues = SpringFilterQuery.GetInValues(filter, "status");
        if (statusInValues.Count > 0)
        {
            var statuses = statusInValues.Select(v => v.ToUpperInvariant()).ToList();
            query = query.Where(r => statuses.Contains(r.Status.ToString().ToUpper()));
        }

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(List<Resume> Items, int Total)> GetByUserPagedAsync(long userId, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Resumes
            .Include(r => r.User)
            .Include(r => r.Job).ThenInclude(j => j.Company)
            .AsNoTracking()
            .Where(r => r.UserId == userId);

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Resume?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Resumes
            .Include(r => r.User)
            .Include(r => r.Job)
                .ThenInclude(j => j.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        _dbContext.Resumes.Add(resume);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        _dbContext.Resumes.Update(resume);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Resumes.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Resumes.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static IQueryable<Resume> ApplySort(IQueryable<Resume> query, string? sort)
    {
        if (!SpringFilterQuery.TryParseSort(sort, out var field, out var desc))
        {
            return query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
        }

        return field.ToLowerInvariant() switch
        {
            "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => desc ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
        };
    }
}
