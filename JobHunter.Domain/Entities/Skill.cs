namespace JobHunter.Domain.Entities;

public class Skill
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<Job> Jobs { get; } = new List<Job>();

    public ICollection<Subscriber> Subscribers { get; } = new List<Subscriber>();
}
