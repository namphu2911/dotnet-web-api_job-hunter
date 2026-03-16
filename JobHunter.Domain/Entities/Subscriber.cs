namespace JobHunter.Domain.Entities;

public class Subscriber
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<Skill> Skills { get; } = new List<Skill>();
}
