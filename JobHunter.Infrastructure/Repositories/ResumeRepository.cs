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

    public async Task<(List<Resume> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Resumes
            .Include(r => r.User)
            .Include(r => r.Job).ThenInclude(j => j.Company)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            query = query.Where(r =>
                (r.Email != null && r.Email.ToLower().Contains(f)) ||
                (r.Job != null && r.Job.Name.ToLower().Contains(f)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(List<Resume> Items, int Total)> GetByUserPagedAsync(long userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Resumes
            .Include(r => r.User)
            .Include(r => r.Job).ThenInclude(j => j.Company)
            .AsNoTracking()
            .Where(r => r.UserId == userId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
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

    public async Task<List<Resume>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Resumes
            .Include(r => r.User)
            .Include(r => r.Job)
                .ThenInclude(j => j.Company)
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
