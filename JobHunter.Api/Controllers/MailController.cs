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
    public async Task<IActionResult> TriggerJobEmails(CancellationToken cancellationToken)
    {
        // Manually trigger the subscriber job dispatch flow (same as EmailJobHostedService)
        // This is a simplified version for manual triggering
        var subscriberService = HttpContext.RequestServices.GetService(typeof(ISubscriberService)) as ISubscriberService;
        var jobRepository = HttpContext.RequestServices.GetService(typeof(JobHunter.Domain.Repositories.IJobRepository)) as JobHunter.Domain.Repositories.IJobRepository;
        if (subscriberService == null || jobRepository == null)
            return StatusCode(500, "Required services not available");

        var subscribers = await subscriberService.GetAllAsync(cancellationToken);
        int sent = 0;
        foreach (var subscriber in subscribers)
        {
            if (subscriber.Skills == null || subscriber.Skills.Count == 0)
                continue;

            var jobs = await jobRepository.FindBySkillsAsync(subscriber.Skills, cancellationToken);

            if (jobs.Count == 0)
                continue;

            var jobList = jobs.Select(j => new { JobName = j.Name, j.Location, j.Description, CompanyName = j.Company.Name }).ToList();

            await _emailService.SendEmailFromTemplateAsync(
                subscriber.Email,
                "New Jobs Matching Your Skills",
                "job.html",
                subscriber.Name,
                new { Jobs = jobList },
                cancellationToken);
            sent++;
        }
        return Ok($"Dispatched job emails to {sent} subscribers.");
    }
}
