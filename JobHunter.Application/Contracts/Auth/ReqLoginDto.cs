using System.ComponentModel.DataAnnotations;

namespace JobHunter.Application.Contracts.Auth;

public sealed class ReqLoginDto
{
    [Required]
    [EmailAddress]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
