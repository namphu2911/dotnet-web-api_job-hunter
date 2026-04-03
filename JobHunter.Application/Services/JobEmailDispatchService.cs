using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Emails;
using JobHunter.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace JobHunter.Application.Services;

public sealed class JobEmailDispatchService : IJobEmailDispatchService
{
    private const string JobEmailTemplate = "job.html";
    private const string JobEmailSubject = "New Jobs Matching Your Skills";

    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<JobEmailDispatchService> _logger;

    public JobEmailDispatchService(
        ISubscriberRepository subscriberRepository,
        IJobRepository jobRepository,
        IEmailService emailService,
        ILogger<JobEmailDispatchService> logger)
    {
        _subscriberRepository = subscriberRepository;
        _jobRepository = jobRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<int> DispatchAsync(CancellationToken cancellationToken = default)
    {
        var subscribers = await _subscriberRepository.GetAllAsync(cancellationToken);
        var sent = 0;

        foreach (var subscriber in subscribers)
        {
            if (string.IsNullOrWhiteSpace(subscriber.Email))
            {
                continue;
            }

            var skillNames = subscriber.Skills
                .Select(s => s.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (skillNames.Count == 0)
            {
                continue;
            }

            var jobs = await _jobRepository.FindBySkillsAsync(skillNames, cancellationToken);
            if (jobs.Count == 0)
            {
                continue;
            }

            var payload = jobs.Select(job => new ResEmailJobDto
            {
                Name = job.Name,
                Salary = Convert.ToDecimal(job.Salary),
                Company = job.Company?.Name ?? string.Empty,
                Skills = job.Skills.Select(skill => skill.Name).ToList()
            }).ToList();

            await _emailService.SendEmailFromTemplateAsync(
                subscriber.Email,
                JobEmailSubject,
                JobEmailTemplate,
                subscriber.Name,
                payload,
                cancellationToken);

            sent++;
        }

        _logger.LogInformation("Dispatched matching job emails to {SentCount} subscribers", sent);
        return sent;
    }
}