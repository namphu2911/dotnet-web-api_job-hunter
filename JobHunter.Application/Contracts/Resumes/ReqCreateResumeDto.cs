namespace JobHunter.Application.Contracts.Resumes;

public class ReqCreateResumeDto
{
    public string Email { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long User { get; set; }
    public long Job { get; set; }
}
