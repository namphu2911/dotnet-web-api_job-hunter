using JobHunter.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobHunter.Infrastructure;

public class EmailJobHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailJobHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

    public EmailJobHostedService(IServiceProvider serviceProvider, ILogger<EmailJobHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var subscriberService = scope.ServiceProvider.GetRequiredService<ISubscriberService>();
                var jobRepository = scope.ServiceProvider.GetRequiredService<JobHunter.Domain.Repositories.IJobRepository>();

                // Get all subscribers
                var subscribers = await subscriberService.GetAllAsync(stoppingToken);
                foreach (var subscriber in subscribers)
                {
                    if (subscriber.Skills == null || subscriber.Skills.Count == 0)
                        continue;

                    // Find jobs matching any of the subscriber's skills
                    var jobs = await jobRepository.FindBySkillsAsync(subscriber.Skills, stoppingToken);

                    if (jobs.Count == 0)
                        continue;

                    // Prepare job list for template
                    var jobList = jobs.Select(j => new { JobName = j.Name, j.Location, j.Description, CompanyName = j.Company.Name }).ToList();

                    // Send email using template (assume job.html exists)
                    await emailService.SendEmailFromTemplateAsync(
                        subscriber.Email,
                        "New Jobs Matching Your Skills",
                        "job.html",
                        subscriber.Name,
                        new { Jobs = jobList },
                        stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running scheduled email job");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
