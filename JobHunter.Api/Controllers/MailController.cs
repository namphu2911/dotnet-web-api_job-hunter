using JobHunter.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public MailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SendTestEmail(CancellationToken cancellationToken)
    {
        // Placeholder: In production, trigger job email to subscribers
        await _emailService.SendSimpleEmailAsync("test@example.com", "Testing Email", "<h1>Hello, this is a test email from Job Hunter application.</h1>", true, cancellationToken);
        return Ok("Email sent successfully!");
    }
}
