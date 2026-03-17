namespace JobHunter.Application.Contracts.Emails;

public class ResEmailJobDto
{
    public string Name { get; set; } = null!;
    public decimal Salary { get; set; }
    public string Company { get; set; } = null!;
    public List<string> Skills { get; set; } = new();
}
