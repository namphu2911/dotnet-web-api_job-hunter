using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface IJobRepository
    {
        Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Job>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Job job, CancellationToken cancellationToken = default);
        Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Job>> FindBySkillsAsync(IEnumerable<string> skills, CancellationToken cancellationToken = default);

        Task<(List<Job> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
