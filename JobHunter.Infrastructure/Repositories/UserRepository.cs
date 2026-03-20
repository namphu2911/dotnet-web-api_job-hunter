using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly JobHunterDbContext _dbContext;

    public UserRepository(JobHunterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.CountAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = null;

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = user.Name;
        existing.Email = user.Email;
        existing.Password = user.Password;
        existing.Age = user.Age;
        existing.Gender = user.Gender;
        existing.Address = user.Address;
        existing.Avatar = user.Avatar;
        existing.CompanyId = user.CompanyId;
        existing.RoleId = user.RoleId;
        existing.RefreshToken = user.RefreshToken;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<User?> GetByRefreshTokenAndEmailAsync(string token, string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x =>
            x.Email == email &&
            x.RefreshToken == token,
            cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x =>
            x.RefreshToken == token,
            cancellationToken);
    }

    public async Task UpdateRefreshTokenAsync(string email, string? refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.RefreshToken = refreshToken;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
