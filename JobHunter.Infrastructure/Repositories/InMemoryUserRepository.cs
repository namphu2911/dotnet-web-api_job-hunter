using System.Collections.Concurrent;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Infrastructure.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<long, User> _users = new();
    private long _sequence;

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user is null ? null : Clone(user));
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user is null ? null : Clone(user));
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var exists = _users.Values.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<User>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var users = _users.Values
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(Clone)
            .ToList();

        return Task.FromResult<IReadOnlyList<User>>(users);
    }

    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Count);
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var clone = Clone(user);
        clone.Id = Interlocked.Increment(ref _sequence);
        clone.CreatedAt = DateTimeOffset.UtcNow;
        clone.UpdatedAt = null;

        _users[clone.Id] = clone;
        return Task.FromResult(Clone(clone));
    }

    public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (!_users.TryGetValue(user.Id, out var current))
        {
            return Task.FromResult<User?>(null);
        }

        var updated = Clone(user);
        updated.CreatedAt = current.CreatedAt;
        updated.UpdatedAt = DateTimeOffset.UtcNow;
        _users[user.Id] = updated;

        return Task.FromResult<User?>(Clone(updated));
    }

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var removed = _users.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<User?> GetByRefreshTokenAndEmailAsync(string token, string email, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(u.RefreshToken, token, StringComparison.Ordinal));

        return Task.FromResult(user is null ? null : Clone(user));
    }

    public Task<User?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u =>
            string.Equals(u.RefreshToken, token, StringComparison.Ordinal));

        return Task.FromResult(user is null ? null : Clone(user));
    }

    public Task UpdateRefreshTokenAsync(string email, string? refreshToken, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        if (user is not null)
        {
            var copy = Clone(user);
            copy.RefreshToken = refreshToken;
            copy.UpdatedAt = DateTimeOffset.UtcNow;
            _users[user.Id] = copy;
        }

        return Task.CompletedTask;
    }

    private static User Clone(User user)
    {
        return new User
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Age = user.Age,
            Gender = user.Gender,
            Address = user.Address,
            Avatar = user.Avatar,
            RefreshToken = user.RefreshToken,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Company = user.Company is null ? null : new NamedEntityReference { Id = user.Company.Id, Name = user.Company.Name },
            Role = user.Role is null ? null : new NamedEntityReference { Id = user.Role.Id, Name = user.Role.Name }
        };
    }
}
