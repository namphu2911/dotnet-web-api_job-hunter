using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Subscribers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobHunter.Api.Authorization;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubscribersController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;

    public SubscribersController(ISubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    [HttpPost]
    [HasPermission("subscriber:create")]
    public async Task<IActionResult> Create([FromBody] ReqCreateSubscriberDto dto, CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }

    [HttpPut]
    [HasPermission("subscriber:update")]
    public async Task<IActionResult> Update([FromBody] ReqUpdateSubscriberDto dto, CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }

    [HttpPost("skills")]
    [Authorize]
    public async Task<IActionResult> GetSkills(CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }
}
