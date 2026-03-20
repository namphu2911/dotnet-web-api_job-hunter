using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class SkillRepository : ISkillRepository
{
    private readonly JobHunterDbContext _dbContext;

    public SkillRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Skill?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Skills
            .AsNoTracking()
            .Include(s => s.Jobs)
            .Include(s => s.Subscribers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Skills
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _dbContext.Skills.Add(skill);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _dbContext.Skills.Update(skill);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Skills.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Skills.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Skills.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<List<Skill>> FindByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Skills.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }
}
