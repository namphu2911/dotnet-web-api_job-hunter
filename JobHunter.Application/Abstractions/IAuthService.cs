using JobHunter.Application.Contracts.Auth;
using JobHunter.Application.Contracts.Users;

namespace JobHunter.Application.Abstractions;

public interface IAuthService
{
    Task<ResLoginDto?> LoginAsync(ReqLoginDto request, CancellationToken cancellationToken = default);

    Task<ResLoginDto.UserGetAccountDto?> GetAccountAsync(string? bearerToken, CancellationToken cancellationToken = default);

    Task<ResLoginDto?> RefreshAsync(string? refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string? bearerToken, CancellationToken cancellationToken = default);

    Task<ResCreateUserDto> RegisterAsync(ReqCreateUserDto request, CancellationToken cancellationToken = default);

    Task<ResLoginDto?> ChangePasswordAsync(string? bearerToken, ReqChangePasswordDto request, CancellationToken cancellationToken = default);
}
