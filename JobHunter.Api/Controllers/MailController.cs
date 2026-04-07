using JobHunter.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MailController : ControllerBase
{
    private readonly IJobEmailDispatchService _jobEmailDispatchService;

    public MailController(IJobEmailDispatchService jobEmailDispatchService)
    {
        _jobEmailDispatchService = jobEmailDispatchService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> TriggerJobEmails(CancellationToken cancellationToken)
    {
        var sent = await _jobEmailDispatchService.DispatchAsync(cancellationToken);
        return Ok($"Dispatched job emails to {sent} subscribers.");
    }
}
