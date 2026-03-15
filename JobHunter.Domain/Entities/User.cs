using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public sealed class User
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public NamedEntityReference? Company { get; set; }

    public NamedEntityReference? Role { get; set; }
}
