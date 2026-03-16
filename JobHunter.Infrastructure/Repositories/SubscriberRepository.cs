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
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Subscriber>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subscribers
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
        return await _dbContext.Subscribers.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}
