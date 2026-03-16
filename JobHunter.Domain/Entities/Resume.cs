using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class Resume
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public ResumeState Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Foreign Keys
    public long UserId { get; set; }

    public long JobId { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    public Job Job { get; set; } = null!;
}
