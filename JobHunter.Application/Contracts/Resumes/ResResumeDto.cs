namespace JobHunter.Application.Contracts.Resumes;

public class ResResumeDto
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? CompanyName { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public long JobId { get; set; }
    public string? JobName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
