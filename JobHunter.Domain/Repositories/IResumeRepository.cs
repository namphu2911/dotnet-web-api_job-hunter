using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface IResumeRepository
    {
        Task<Resume?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Resume>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Resume resume, CancellationToken cancellationToken = default);
        Task UpdateAsync(Resume resume, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
