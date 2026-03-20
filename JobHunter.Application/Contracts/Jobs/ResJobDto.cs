namespace JobHunter.Application.Contracts.Jobs;

public class ResJobDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public decimal Salary { get; set; }
    public int Quantity { get; set; }
    public string Level { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<string> Skills { get; set; } = new();
}
