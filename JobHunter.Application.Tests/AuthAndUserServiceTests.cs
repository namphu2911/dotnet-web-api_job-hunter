using JobHunter.Application.Contracts.Auth;
using JobHunter.Application.Contracts.Users;
using JobHunter.Application.Abstractions;
using JobHunter.Application.Services;
using JobHunter.Domain.Enums;
using JobHunter.Domain.Entities;
using JobHunter.Infrastructure.Repositories;

namespace JobHunter.Application.Tests;

public class AuthAndUserServiceTests
{
    [Fact]
    public async Task CreateUserAsync_StoresPasswordAsBcryptHash()
    {
        var (userService, _, repository) = CreateServices();

        const string plainPassword = "Password123";
        await userService.CreateUserAsync(new ReqCreateUserDto
        {
            Name = "Hash Check",
            Email = "hash-check@example.com",
            Password = plainPassword,
            Age = 22,
            Gender = Gender.Male
        });

        var storedUser = await repository.GetByEmailAsync("hash-check@example.com");

        Assert.NotNull(storedUser);
        Assert.NotEqual(plainPassword, storedUser!.PasswordHash);
        Assert.StartsWith("$2", storedUser.PasswordHash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        var (userService, _, _) = CreateServices();

        var request = new ReqCreateUserDto
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "Password123",
            Age = 28,
            Gender = Gender.Female,
            Address = "Hanoi"
        };

        await userService.CreateUserAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(() => userService.CreateUserAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        var (userService, authService, _) = CreateServices();

        await userService.CreateUserAsync(new ReqCreateUserDto
        {
            Name = "Bob",
            Email = "bob@example.com",
            Password = "Password123",
            Age = 30,
            Gender = Gender.Male
        });

        var loginResult = await authService.LoginAsync(new ReqLoginDto
        {
            Username = "bob@example.com",
            Password = "Password123"
        });

        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrWhiteSpace(loginResult!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginResult.RefreshTokenInternal));
        Assert.Equal("bob@example.com", loginResult.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        var (userService, authService, _) = CreateServices();

        await userService.CreateUserAsync(new ReqCreateUserDto
        {
            Name = "Carol",
            Email = "carol@example.com",
            Password = "Password123",
            Age = 24,
            Gender = Gender.Female
        });

        var loginResult = await authService.LoginAsync(new ReqLoginDto
        {
            Username = "carol@example.com",
            Password = "WrongPassword"
        });

        Assert.Null(loginResult);
    }

    [Fact]
    public async Task RefreshAsync_WithValidRefreshToken_ReturnsNewAccessToken()
    {
        var (userService, authService, _) = CreateServices();

        await userService.CreateUserAsync(new ReqCreateUserDto
        {
            Name = "David",
            Email = "david@example.com",
            Password = "Password123",
            Age = 35,
            Gender = Gender.Male
        });

        var loginResult = await authService.LoginAsync(new ReqLoginDto
        {
            Username = "david@example.com",
            Password = "Password123"
        });

        var refreshed = await authService.RefreshAsync(loginResult!.RefreshTokenInternal);

        Assert.NotNull(refreshed);
        Assert.NotEqual(loginResult.AccessToken, refreshed!.AccessToken);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshTokenInternal));
    }

    private static (UserManagementService userManagementService, AuthService authService, InMemoryUserRepository repository) CreateServices()
    {
        var repository = new InMemoryUserRepository();
        var userManagementService = new UserManagementService(repository);
        var authService = new AuthService(repository, userManagementService, new FakeJwtTokenService());
        return (userManagementService, authService, repository);
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public string CreateAccessToken(User user, IReadOnlyCollection<string> permissions)
        {
            return $"token-{user.Email}-{Guid.NewGuid():N}";
        }

        public string CreateRefreshToken(User user)
        {
            return $"refresh-{user.Email}-{Guid.NewGuid():N}";
        }

        public bool TryGetEmailFromAccessToken(string token, out string? email)
        {
            email = token.Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .FirstOrDefault();

            return !string.IsNullOrWhiteSpace(email);
        }

        public bool TryGetEmailFromRefreshToken(string token, out string? email)
        {
            email = token.Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .FirstOrDefault();

            return !string.IsNullOrWhiteSpace(email);
        }
    }
}
