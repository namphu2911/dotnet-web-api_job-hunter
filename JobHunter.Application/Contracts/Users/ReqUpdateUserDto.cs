using System.ComponentModel.DataAnnotations;
using JobHunter.Application.Contracts;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Contracts.Users;

public sealed class ReqUpdateUserDto
{
    [Range(1, long.MaxValue)]
    public long Id { get; set; }

    public string? Name { get; set; }

    [Range(0, 120)]
    public int Age { get; set; }

    public Gender Gender { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public ReqObjectIdDto? Company { get; set; }

    public ReqObjectIdDto? Role { get; set; }
}
