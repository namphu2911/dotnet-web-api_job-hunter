using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface ISubscriberRepository
    {
        Task<Subscriber?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Subscriber>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Subscriber subscriber, CancellationToken cancellationToken = default);
        Task UpdateAsync(Subscriber subscriber, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Subscriber?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
