using System.ComponentModel.DataAnnotations;

namespace JobHunter.Application.Contracts.Auth;

public sealed class ReqChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
