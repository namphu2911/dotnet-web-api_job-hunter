namespace JobHunter.Application.Contracts.Subscribers;

public class ReqCreateSubscriberDto
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<ReqObjectIdDto> Skills { get; set; } = new();
}
