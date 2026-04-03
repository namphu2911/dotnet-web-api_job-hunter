namespace JobHunter.Application.Abstractions;

public interface IJobEmailDispatchService
{
    Task<int> DispatchAsync(CancellationToken cancellationToken = default);
}