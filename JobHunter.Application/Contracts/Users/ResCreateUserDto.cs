using JobHunter.Application.Contracts;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Contracts.Users;

public sealed class ResCreateUserDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public string? Address { get; set; }

    public int Age { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ResObjectIdNameDto? Company { get; set; }
}
