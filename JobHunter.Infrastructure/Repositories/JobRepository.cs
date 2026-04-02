using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly JobHunterDbContext _dbContext;

    public JobRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<Job> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Jobs
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .AsNoTracking()
            .AsQueryable();

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "name"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(j => j.Name.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "location"))
        {
            var term = value.ToLowerInvariant();
            query = query.Where(j => j.Location.ToLower().Contains(term));
        }

        foreach (var value in SpringFilterQuery.GetContainsValues(filter, "salary"))
        {
            if (double.TryParse(value, out var salary))
            {
                query = query.Where(j => j.Salary == salary);
            }
        }

        var levelInValues = SpringFilterQuery.GetInValues(filter, "level");
        if (levelInValues.Count > 0)
        {
            var levels = levelInValues.Select(v => v.ToUpperInvariant()).ToList();
            query = query.Where(j => levels.Contains(j.Level.ToString().ToUpper()));
        }

        var locationInValues = SpringFilterQuery.GetInValues(filter, "location");
        if (locationInValues.Count > 0)
        {
            var locations = locationInValues.Select(v => v.ToUpperInvariant()).ToList();
            query = query.Where(j => locations.Contains(j.Location.ToUpper()));
        }

        var skillInValues = SpringFilterQuery.GetInValues(filter, "skills");
        if (skillInValues.Count > 0)
        {
            var skills = skillInValues.Select(v => v.ToUpperInvariant()).ToList();
            query = query.Where(j => j.Skills.Any(s => skills.Contains(s.Id.ToString())));
        }

        query = ApplySort(query, sort);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Jobs
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        _dbContext.Jobs.Update(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Jobs.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Jobs.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<Job>> FindBySkillsAsync(IEnumerable<string> skills, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Jobs
            .Include(j => j.Company)
            .Where(j => j.Skills.Any(s => skills.Contains(s.Name)))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Job> ApplySort(IQueryable<Job> query, string? sort)
    {
        if (!SpringFilterQuery.TryParseSort(sort, out var field, out var desc))
        {
            return query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
        }

        return field.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "salary" => desc ? query.OrderByDescending(x => x.Salary) : query.OrderBy(x => x.Salary),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" => desc ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
        };
    }
}
