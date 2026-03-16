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

    public async Task<Resume?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Resume>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Resumes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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
}
