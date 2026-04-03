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
                var dispatchService = scope.ServiceProvider.GetRequiredService<IJobEmailDispatchService>();
                await dispatchService.DispatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running scheduled email job");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
