using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class Job
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public double Salary { get; set; }

    public int Quantity { get; set; }

    public Level Level { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Foreign Keys
    public long CompanyId { get; set; }

    // Navigation Properties
    public Company Company { get; set; } = null!;

    public ICollection<Skill> Skills { get; } = new List<Skill>();

    public ICollection<Resume> Resumes { get; } = new List<Resume>();
}
