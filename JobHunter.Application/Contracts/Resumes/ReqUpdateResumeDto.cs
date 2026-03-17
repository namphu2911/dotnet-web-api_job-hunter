namespace JobHunter.Application.Contracts.Resumes;

public class ReqUpdateResumeDto
{
    public long Id { get; set; }
    public string Status { get; set; } = null!;
}
