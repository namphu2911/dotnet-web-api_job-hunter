using System.Text.Json.Serialization;

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

        public RoleDto? Role { get; set; }
    }

    public sealed class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IReadOnlyList<PermissionDto> Permissions { get; set; } = Array.Empty<PermissionDto>();
    }

    public sealed class PermissionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiPath { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }

    public sealed class UserGetAccountDto
    {
        public UserLoginDto User { get; set; } = new();
    }
}
