using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<User?> GetByRefreshTokenAndEmailAsync(string token, string email, CancellationToken cancellationToken = default);

    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    Task UpdateRefreshTokenAsync(string email, string? refreshToken, CancellationToken cancellationToken = default);
}
