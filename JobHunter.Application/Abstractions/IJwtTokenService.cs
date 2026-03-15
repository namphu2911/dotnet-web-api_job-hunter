using JobHunter.Domain.Entities;

namespace JobHunter.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateAccessToken(User user, IReadOnlyCollection<string> permissions);

    string CreateRefreshToken(User user);

    bool TryGetEmailFromAccessToken(string token, out string? email);

    bool TryGetEmailFromRefreshToken(string token, out string? email);
}
