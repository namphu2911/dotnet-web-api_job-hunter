namespace JobHunter.Application.Contracts.Subscribers;

public class ReqUpdateSubscriberDto
{
    public long Id { get; set; }
    public List<long> Skills { get; set; } = new();
}
