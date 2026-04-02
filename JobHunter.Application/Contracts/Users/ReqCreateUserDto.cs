using System.ComponentModel.DataAnnotations;
using JobHunter.Application.Contracts;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Contracts.Users;

public sealed class ReqCreateUserDto
{
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Range(0, 120)]
    public int Age { get; set; }

    public Gender Gender { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public ReqObjectIdDto? Company { get; set; }

    public ReqObjectIdDto? Role { get; set; }
}
