using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class User
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Foreign Keys
    public long? CompanyId { get; set; }
    public long? RoleId { get; set; }

    // Navigation Properties
    public Company? Company { get; set; }

    public Role? Role { get; set; }

    public ICollection<Resume> Resumes { get; } = new List<Resume>();
}
