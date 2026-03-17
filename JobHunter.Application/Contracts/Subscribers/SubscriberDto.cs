namespace JobHunter.Application.Contracts.Subscribers;

public class SubscriberDto
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> Skills { get; set; } = new();
}
