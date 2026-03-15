namespace JobHunter.Domain.Entities;

public sealed class NamedEntityReference
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
