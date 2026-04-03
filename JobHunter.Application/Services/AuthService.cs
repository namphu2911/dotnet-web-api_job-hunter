using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Auth;
using JobHunter.Application.Contracts.Users;
using JobHunter.Application.Services.Security;
using JobHunter.Application.Utilities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserManagementService _userManagementService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, IUserManagementService userManagementService, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _userManagementService = userManagementService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ResLoginDto?> LoginAsync(ReqLoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Username, cancellationToken);
        if (user is null || !PasswordSecurity.VerifyPassword(request.Password, user.Password))
        {
            return null;
        }

        var accessToken = _jwtTokenService.CreateAccessToken(user, ResolvePermissions(user));
        var refreshToken = _jwtTokenService.CreateRefreshToken(user);
        await _userRepository.UpdateRefreshTokenAsync(user.Email, refreshToken, cancellationToken);

        return BuildLoginResponse(user, accessToken, refreshToken);
    }

    public async Task<ResLoginDto.UserGetAccountDto?> GetAccountAsync(string? bearerToken, CancellationToken cancellationToken = default)
    {
        var email = TryGetEmailFromBearer(bearerToken);
        if (email is null)
        {
            return null;
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new ResLoginDto.UserGetAccountDto
        {
            User = new ResLoginDto.UserLoginDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Avatar = user.Avatar,
                Role = MapRole(user)
            }
        };
    }

    public async Task<ResLoginDto?> RefreshAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        if (!_jwtTokenService.TryGetEmailFromRefreshToken(refreshToken, out var email) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var user = await _userRepository.GetByRefreshTokenAndEmailAsync(refreshToken, email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var newAccessToken = _jwtTokenService.CreateAccessToken(user, ResolvePermissions(user));
        var newRefreshToken = _jwtTokenService.CreateRefreshToken(user);
        await _userRepository.UpdateRefreshTokenAsync(user.Email, newRefreshToken, cancellationToken);

        return BuildLoginResponse(user, newAccessToken, newRefreshToken);
    }

    public async Task LogoutAsync(string? bearerToken, CancellationToken cancellationToken = default)
    {
        var email = TryGetEmailFromBearer(bearerToken);
        if (email is null)
        {
            return;
        }

        await _userRepository.UpdateRefreshTokenAsync(email, null, cancellationToken);
    }

    public Task<ResCreateUserDto> RegisterAsync(ReqCreateUserDto request, CancellationToken cancellationToken = default)
    {
        return _userManagementService.CreateUserAsync(request, cancellationToken);
    }

    public async Task<ResLoginDto?> ChangePasswordAsync(string? bearerToken, ReqChangePasswordDto request, CancellationToken cancellationToken = default)
    {
        var email = TryGetEmailFromBearer(bearerToken);
        if (email is null)
        {
            return null;
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !PasswordSecurity.VerifyPassword(request.OldPassword, user.Password))
        {
            return null;
        }

        user.Password = PasswordSecurity.HashPassword(request.NewPassword);
        var newRefreshToken = _jwtTokenService.CreateRefreshToken(user);
        var newAccessToken = _jwtTokenService.CreateAccessToken(user, ResolvePermissions(user));

        user.RefreshToken = newRefreshToken;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return BuildLoginResponse(user, newAccessToken, newRefreshToken);
    }

    private ResLoginDto BuildLoginResponse(JobHunter.Domain.Entities.User user, string accessToken, string refreshToken)
    {
        return new ResLoginDto
        {
            AccessToken = accessToken,
            RefreshTokenInternal = refreshToken,
            User = new ResLoginDto.UserLoginDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Avatar = user.Avatar,
                Role = MapRole(user)
            }
        };
    }

    private static ResLoginDto.RoleDto? MapRole(JobHunter.Domain.Entities.User user)
    {
        if (user.Role is null)
        {
            return null;
        }

        return new ResLoginDto.RoleDto
        {
            Id = user.Role.Id,
            Name = user.Role.Name,
            Permissions = user.Role.Permissions
                .Select(permission => new ResLoginDto.PermissionDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    ApiPath = permission.ApiPath,
                    Method = permission.Method,
                    Module = permission.Module
                })
                .ToList()
        };
    }

    private string? TryGetEmailFromBearer(string? bearerHeader)
    {
        var token = ExtractBearerToken(bearerHeader);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return _jwtTokenService.TryGetEmailFromAccessToken(token, out var email) ? email : null;
    }

    private static IReadOnlyCollection<string> ResolvePermissions(JobHunter.Domain.Entities.User user)
    {
        // Grant wildcard for both ADMIN and SUPER_ADMIN
        if (string.Equals(user.Role?.Name, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(user.Role?.Name, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "*" };
        }

        // Otherwise, return permissions from role
        if (user.Role?.Permissions != null && user.Role.Permissions.Count > 0)
        {
            return user.Role.Permissions
                .Select(p => PermissionClaimValue.Encode(p.Method, p.ApiPath, p.Module))
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToArray();
        }
        return Array.Empty<string>();
    }

    private static string? ExtractBearerToken(string? bearerHeader)
    {
        if (string.IsNullOrWhiteSpace(bearerHeader))
        {
            return null;
        }

        const string bearerPrefix = "Bearer ";
        return bearerHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? bearerHeader[bearerPrefix.Length..].Trim()
            : null;
    }
}