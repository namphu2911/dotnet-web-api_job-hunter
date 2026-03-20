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

    public async Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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
}
