using System.Text.Json.Serialization;
using JobHunter.Application.Contracts.Users;

namespace JobHunter.Application.Contracts.Auth;

public sealed class ResLoginDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string? RefreshTokenInternal { get; set; }

    public UserLoginDto User { get; set; } = new();

    public sealed class UserLoginDto
    {
        public long Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Avatar { get; set; }

        public ResObjectIdNameDto? Role { get; set; }
    }

    public sealed class UserGetAccountDto
    {
        public UserLoginDto User { get; set; } = new();
    }
}
