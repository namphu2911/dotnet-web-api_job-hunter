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

    public async Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Jobs
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

    public async Task<List<Job>> FindBySkillsAsync(IEnumerable<Skill> skills, CancellationToken cancellationToken = default)
    {
        var skillIds = skills.Select(s => s.Id).ToList();
        return await _dbContext.Jobs
            .Where(j => j.Skills.Any(s => skillIds.Contains(s.Id)))
            .ToListAsync(cancellationToken);
    }
}
