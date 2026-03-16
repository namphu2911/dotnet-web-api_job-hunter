namespace JobHunter.Domain.Entities;

public class Permission
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ApiPath { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<Role> Roles { get; } = new List<Role>();
}
