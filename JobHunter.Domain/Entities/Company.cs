namespace JobHunter.Domain.Entities;

public class Company
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public string? Logo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<User> Users { get; } = new List<User>();

    public ICollection<Job> Jobs { get; } = new List<Job>();
}
