namespace JobHunter.Application.Contracts.Resumes;

public class ResCreateResumeDto
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
