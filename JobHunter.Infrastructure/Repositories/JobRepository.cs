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

    public async Task<(List<Job> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Jobs
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            query = query.Where(j =>
                (j.Name != null && j.Name.ToLower().Contains(f)) ||
                (j.Location != null && j.Location.ToLower().Contains(f)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
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

    public async Task<List<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Jobs
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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
}
