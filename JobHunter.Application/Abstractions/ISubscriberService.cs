using JobHunter.Application.Contracts.Subscribers;

namespace JobHunter.Application.Abstractions;

public interface ISubscriberService
{
    Task<long> CreateAsync(ReqCreateSubscriberDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReqUpdateSubscriberDto dto, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<SubscriberDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<SubscriberDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
