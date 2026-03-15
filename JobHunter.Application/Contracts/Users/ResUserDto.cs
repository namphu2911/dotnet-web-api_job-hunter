using JobHunter.Domain.Enums;

namespace JobHunter.Application.Contracts.Users;

public sealed class ResUserDto
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public int Age { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ResObjectIdNameDto? Company { get; set; }

    public ResObjectIdNameDto? Role { get; set; }
}
