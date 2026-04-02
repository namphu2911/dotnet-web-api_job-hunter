using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Application.Contracts.Subscribers;
using JobHunter.Api.Contracts;
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
        try
        {
            var id = await _subscriberService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ResultPaginationDto<SubscriberDto>>> GetList([FromQuery] ListQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _subscriberService.GetListAsync(query.Filter, query.ResolvePage(), query.ResolvePageSize(), query.Sort, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken cancellationToken)
    {
        var subscriber = await _subscriberService.GetByIdAsync(id, cancellationToken);
        if (subscriber == null)
            return NotFound(new { message = $"Subscriber with id {id} not found" });

        return Ok(subscriber);
    }

    [HttpPut]
    [HasPermission("subscriber:update")]
    public async Task<IActionResult> Update([FromBody] ReqUpdateSubscriberDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _subscriberService.UpdateAsync(dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            await _subscriberService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("skills")]
    [Authorize]
    public async Task<IActionResult> GetSkills(CancellationToken cancellationToken)
    {
        // Extract email from claims (assume email claim type)
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == "email" || c.Type.EndsWith("/emailaddress"));
        if (emailClaim == null)
            return Unauthorized(new { message = "Email not found in token" });
        var subscriber = await _subscriberService.GetByEmailAsync(emailClaim.Value, cancellationToken);
        if (subscriber == null)
            return NotFound(new { message = $"Subscriber with email {emailClaim.Value} not found" });
        return Ok(subscriber.Skills);
    }
}
