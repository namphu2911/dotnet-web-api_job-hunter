namespace JobHunter.Application.Contracts.Subscribers;

public class ReqUpdateSubscriberDto
{
    public long Id { get; set; }
    public List<ReqObjectIdDto> Skills { get; set; } = new();
}
